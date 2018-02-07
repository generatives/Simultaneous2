using Hyperion;
using LiteNetLib;
using SimultaneousNetwork.Object;
using SimultaneousNetwork.Object.Proxy;
using SimultaneousNetwork.SubSpace;
using SimultaneousNetwork.SubSpace.Messages;
using SimultaneousNetwork.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SimultaneousNetwork
{
    public enum MemberState
    {
        SEEDING, JOINING, JOINED
    }

    public class NetworkMember
    {
        public Guid Id { get; private set; }
        
        private Serializer _serializer;

        private EventBasedNetListener _listener;
        private NetManager _netManager;

        private Dictionary<Guid, RemoteMember> _remoteMemberIds;
        public IReadOnlyDictionary<Guid, RemoteMember> RemoteMemberIds => _remoteMemberIds;

        private ObjectSpace _objSpace;

        public MemberState State { get; set; }

        public static NetworkMember Start(ObjectSpace space, int port, int maxMembers = 100, string connectionKey = "SimultaneousNetwork")
        {
            var pool = new NetworkMember(space, Guid.NewGuid(), maxMembers, connectionKey);
            pool.State = MemberState.SEEDING;
            pool._netManager.Start(port);

            return pool;
        }

        public static NetworkMember Join(ObjectSpace space, string seedAddress, int seedPort, int maxMembers = 100, string connectionKey = "SimultaneousNetwork")
        {
            var pool = new NetworkMember(space, maxMembers, connectionKey);
            pool.State = MemberState.JOINING;
            pool._netManager.Start();
            pool._netManager.Connect(seedAddress, seedPort);
            
            return pool;
        }

        private NetworkMember(ObjectSpace space, Guid id, int maxMembers, string connectionString) : this(space, maxMembers, connectionString)
        {
            Id = id;
            space.MemberIdAssigned(Id);
        }

        private NetworkMember(ObjectSpace space, int maxMembers, string connectionKey)
        {
            _objSpace = space;

            _remoteMemberIds = new Dictionary<Guid, RemoteMember>();
            _listener = new EventBasedNetListener();
            _netManager = new NetManager(_listener, maxMembers, connectionKey);

            _listener.PeerConnectedEvent += _listener_PeerConnectedEvent;
            _listener.NetworkReceiveEvent += _listener_NetworkReceiveEvent;
            
            _remoteMemberIds = new Dictionary<Guid, RemoteMember>();
            Surrogate[] Surrogates =
            {
                Surrogate.Create<INetObj, NetObjSurrogate>(
                    obj => new NetObjSurrogate(obj.Id, obj.SubSpace),
                    objRef => objRef.Member[objRef.Id]),
                Surrogate.Create<ISubSpace, NetMemberSurrogate>(
                    mem => new NetMemberSurrogate(mem.MemberId),
                    spaceRef => _objSpace.GetSubSpace(spaceRef.Id))
            };
            SerializerOptions options = new SerializerOptions(surrogates: Surrogates);
            _serializer = new Serializer(options);
        }

        internal void Update()
        {
            foreach (var member in _remoteMemberIds.Values)
            {
                member.Update();
            }
            _netManager.PollEvents();
        }

        internal void NetObjRemoved(INetObj obj)
        {
            TellRemotes(new ObjectRemoved()
            {
                Obj = obj
            });
        }

        internal void NetObjAdded(INetObj obj)
        {
            TellRemotes(new ObjectAdded()
            {
                Description = new NetObjDescription()
                {
                    Id = obj.Id,
                    Traits = obj.Traits,
                    MemberId = Id
                }
            });
        }

        private void _listener_NetworkReceiveEvent(NetPeer peer, LiteNetLib.Utils.NetDataReader reader)
        {
            if(reader.EndOfData || !reader.EndOfData)
            {
                var messages = GetMessage(reader.Data) as object[];
                foreach (var message in messages)
                {
                    switch (message)
                    {
                        case RequestOnBoarding reqOnBoard:
                            if (State == MemberState.SEEDING)
                            {
                                var id = Guid.NewGuid();
                                peer.Tag = id;
                                var onBoarding = new OnBoarding()
                                {
                                    AssignedId = id,
                                    SeedId = Id,
                                    Members = _remoteMemberIds.Select(kvp => kvp.Value.GetRef()).ToArray()
                                };
                                var newRemote = AddRemoteMember(id, peer);
                                newRemote.Tell(onBoarding);
                            }
                            break;
                        case OnBoarding onBoarding:
                            if (State == MemberState.JOINING)
                            {
                                Id = onBoarding.AssignedId;
                                _objSpace.MemberIdAssigned(Id);
                                var memJoined = new MemberJoined() { Id = onBoarding.AssignedId };
                                foreach (var info in onBoarding.Members)
                                {
                                    var newPeer = _netManager.Connect(info.Address, info.Port);
                                    var member = AddRemoteMember(info.Id, newPeer);
                                    member.Tell(memJoined);
                                }
                                var hostMember = AddRemoteMember(onBoarding.SeedId, peer);
                                State = MemberState.JOINED;
                                hostMember.Tell(new FinishedOnBoarding());
                            }
                            break;
                        case MemberJoined memJoined:
                            AddRemoteMember(memJoined.Id, peer);
                            break;
                        case MemberMessage memMess:
                            memMess.Member.Tell(memMess.Message);
                            break;
                        case ObjectMessage objMess:
                            objMess.Object.Tell(objMess.Sender, objMess.Message);
                            break;
                        case ObjectAdded objAdded:
                            var desc = objAdded.Description;
                            _objSpace.RemoteNetObjAdded(desc);
                            break;
                        case ObjectRemoved objRemoved:
                            var remObj = objRemoved.Obj;
                            _objSpace.RemoteNetObjRemoved(remObj.Id, remObj.SubSpace.MemberId);
                            break;
                    }
                }
            }
        }

        private void _listener_PeerConnectedEvent(NetPeer peer)
        {
            if (State == MemberState.JOINING)
            {
                peer.Send(new object[] { new RequestOnBoarding() }, _serializer);
            }
        }

        private void TellRemotes(object message)
        {
            foreach (var remote in _remoteMemberIds.Values)
            {
                remote.Tell(message);
            }
        }

        private object GetMessage(byte[] message)
        {
            using (var stream = new MemoryStream(message))
            {
                return _serializer.Deserialize(stream);
            }
        }

        private RemoteMember AddRemoteMember(Guid id, NetPeer peer)
        {
            var member = new RemoteMember(id, peer, _serializer);
            var space = new RemoteSubSpace(member, _objSpace);
            _remoteMemberIds[id] = member;
            _objSpace.RemoteSpaceAdded(space);
            return member;
        }
    }
}
