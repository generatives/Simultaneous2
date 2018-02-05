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

        public Dictionary<Guid, RemoteMember> RemoteMemberIds { get; private set; }
        public LocalSubSpace LocalSpace { get; private set; }

        private List<Query> _queries;
        private Serializer _serializer;

        private EventBasedNetListener _listener;
        private NetManager _netManager;

        public MemberState State { get; set; }

        public IEnumerable<INetObj> NetObjs
        {
            get
            {
                foreach(var remote in RemoteMemberIds.Values)
                {
                    foreach(var obj in remote.Space.NetObjs)
                    {
                        yield return obj;
                    }
                }
                if(LocalSpace != null)
                {
                    foreach (var obj in LocalSpace.NetObjs)
                    {
                        yield return obj;
                    }
                }
            }
        }

        public static NetworkMember Start(int port, int maxMembers = 100, string connectionKey = "SimultaneousNetwork")
        {
            var pool = new NetworkMember(Guid.NewGuid(), maxMembers, connectionKey);
            pool.State = MemberState.SEEDING;
            pool._netManager.Start(port);

            return pool;
        }

        public static NetworkMember Join(string seedAddress, int seedPort, int maxMembers = 100, string connectionKey = "SimultaneousNetwork")
        {
            var pool = new NetworkMember(maxMembers, connectionKey);
            pool.State = MemberState.JOINING;
            pool._netManager.Start();
            pool._netManager.Connect(seedAddress, seedPort);
            
            return pool;
        }

        private NetworkMember(Guid id, int maxMembers, string connectionString) : this(maxMembers, connectionString)
        {
            _queries = new List<Query>();
            Id = id;
            LocalSpace = new LocalSubSpace(this);
        }

        private NetworkMember(int maxMembers, string connectionKey)
        {
            _queries = new List<Query>();
            _listener = new EventBasedNetListener();
            _netManager = new NetManager(_listener, maxMembers, connectionKey);

            _listener.PeerConnectedEvent += _listener_PeerConnectedEvent;
            _listener.NetworkReceiveEvent += _listener_NetworkReceiveEvent;
            
            RemoteMemberIds = new Dictionary<Guid, RemoteMember>();
            Surrogate[] Surrogates =
            {
                Surrogate.Create<INetObj, NetObjSurrogate>(
                    obj => new NetObjSurrogate(obj.Id, obj.Member),
                    objRef => objRef.Member[objRef.Id]),
                Surrogate.Create<ISubSpace, NetMemberSurrogate>(
                    mem => new NetMemberSurrogate(mem.MemberId),
                    objRef => (objRef.Id == LocalSpace.MemberId ? LocalSpace as ISubSpace : RemoteMemberIds[objRef.Id].Space))
            };
            SerializerOptions options = new SerializerOptions(surrogates: Surrogates);
            _serializer = new Serializer(options);
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
                                    SeedId = LocalSpace.MemberId,
                                    Members = RemoteMemberIds.Select(kvp => kvp.Value.GetRef()).ToArray()
                                };
                                var newRemote = new RemoteMember(id, peer, _serializer);
                                RemoteMemberIds[id] = newRemote;
                                newRemote.Tell(onBoarding);
                            }
                            break;
                        case OnBoarding onBoarding:
                            if (State == MemberState.JOINING)
                            {
                                Id = onBoarding.AssignedId;
                                LocalSpace = new LocalSubSpace(this);
                                var memJoined = new MemberJoined() { Id = onBoarding.AssignedId };
                                foreach (var info in onBoarding.Members)
                                {
                                    var newPeer = _netManager.Connect(info.Address, info.Port);
                                    var member = new RemoteMember(info.Id, newPeer, _serializer);
                                    RemoteMemberIds[info.Id] = member;
                                    member.Tell(memJoined);
                                }
                                var hostMember = new RemoteMember(onBoarding.SeedId, peer, _serializer);
                                RemoteMemberIds[onBoarding.SeedId] = hostMember;
                                State = MemberState.JOINED;
                                hostMember.Tell(new FinishedOnBoarding());
                            }
                            break;
                        case MemberJoined memJoined:
                            RemoteMemberIds[memJoined.Id] = new RemoteMember(memJoined.Id, peer, _serializer);
                            break;
                        case MemberMessage memMess:
                            memMess.Member.Tell(memMess.Message);
                            break;
                        case ObjectMessage objMess:
                            objMess.Object.Tell(objMess.Sender, objMess.Message);
                            break;
                        case ObjectAdded objAdded:
                            var desc = objAdded.Description;
                            var obj = RemoteMemberIds[desc.MemberId].Space.AddObj(desc);
                            foreach(var query in _queries)
                            {
                                query.ObjAdded(obj);
                            }
                            break;
                        case ObjectRemoved objRemoved:
                            var remObj = objRemoved.Obj;
                            var remote = RemoteMemberIds[remObj.Member.MemberId];
                            remote.Space.RemoveObj(remObj);
                            foreach (var query in _queries)
                            {
                                query.ObjRemoved(remObj);
                            }
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

        public void Update()
        {
            foreach(var member in RemoteMemberIds.Values)
            {
                member.Update();
            }
            _netManager.PollEvents();
        }

        public void AddNetObj(Func<Guid, ISubSpace, NetObj> factory)
        {
            var obj = factory(Guid.NewGuid(), LocalSpace);
            LocalSpace.AddNetObj(obj);

            TellRemotes(new ObjectAdded()
            {
                Description = new NetObjDescription()
                {
                    Id = obj.Id,
                    Traits = obj.Traits,
                    MemberId = Id
                }
            });

            foreach (var query in _queries)
            {
                query.ObjAdded(obj);
            }
        }

        public void RemoveNetObj(INetObj obj)
        {
            var removed = LocalSpace.RemoveNetObj(obj.Id);

            if(removed)
            {
                TellRemotes(new ObjectRemoved()
                {
                    Obj = obj
                });

                foreach (var query in _queries)
                {
                    query.ObjRemoved(obj);
                }
            }
        }

        public Query AddQuery(Func<INetObj, bool> exp)
        {
            var query = new Query(exp);
            _queries.Add(query);
            return query;
        }

        public void RemoveQuery(Query query)
        {
            _queries.Remove(query);
        }

        private void TellRemotes(object message)
        {
            foreach (var remote in RemoteMemberIds.Values)
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
    }
}
