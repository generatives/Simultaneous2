using Hyperion;
using LiteNetLib;
using SimultaneousNetwork.Object;
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
        public Dictionary<Guid, RemoteSubSpace> RemoteMemberIds { get; private set; }
        public LocalSubSpace LocalMember { get; private set; }
        private Serializer _serializer;

        private EventBasedNetListener _listener;
        private NetManager _netManager;

        public MemberState State { get; set; }

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
            LocalMember = new LocalSubSpace(Guid.NewGuid());
        }

        private NetworkMember(int maxMembers, string connectionKey)
        {
            _listener = new EventBasedNetListener();
            _netManager = new NetManager(_listener, maxMembers, connectionKey);

            _listener.PeerConnectedEvent += _listener_PeerConnectedEvent;
            _listener.NetworkReceiveEvent += _listener_NetworkReceiveEvent;
            
            RemoteMemberIds = new Dictionary<Guid, RemoteSubSpace>();
            Surrogate[] Surrogates =
            {
                Surrogate.Create<INetObj, NetObjSurrogate>(
                    obj => new NetObjSurrogate(obj.Id, obj.Member),
                    objRef => objRef.Member.GetObj(objRef.Id)),
                Surrogate.Create<ISubSpace, NetMemberSurrogate>(
                    mem => new NetMemberSurrogate(mem.Id),
                    objRef => (objRef.Id == LocalMember.Id ? LocalMember as ISubSpace : RemoteMemberIds[objRef.Id]))
            };
            SerializerOptions options = new SerializerOptions(surrogates: Surrogates);
            _serializer = new Serializer(options);
        }

        private void _listener_NetworkReceiveEvent(NetPeer peer, LiteNetLib.Utils.NetDataReader reader)
        {
            var messages = GetMessage(reader.Data) as object[];
            foreach(var message in messages)
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
                                SeedId = LocalMember.Id,
                                Members = RemoteMemberIds.Select(kvp => kvp.Value.GetRef()).ToArray()
                            };
                            var newRemote = new RemoteSubSpace(id, peer, _serializer);
                            RemoteMemberIds[id] = newRemote;
                            newRemote.Tell(onBoarding);
                        }
                        break;
                    case OnBoarding onBoarding:
                        if (State == MemberState.JOINING)
                        {
                            LocalMember = new LocalSubSpace(onBoarding.AssignedId);
                            var memJoined = new MemberJoined() { Id = onBoarding.AssignedId };
                            foreach (var info in onBoarding.Members)
                            {
                                var newPeer = _netManager.Connect(info.Address, info.Port);
                                var member = new RemoteSubSpace(info.Id, newPeer, _serializer);
                                RemoteMemberIds[info.Id] = member;
                                member.Tell(memJoined);
                            }
                            var hostMember = new RemoteSubSpace(onBoarding.SeedId, peer, _serializer);
                            RemoteMemberIds[onBoarding.SeedId] = hostMember;
                            State = MemberState.JOINED;
                            hostMember.Tell(new FinishedOnBoarding());
                        }
                        break;
                    case MemberJoined memJoined:
                        RemoteMemberIds[memJoined.Id] = new RemoteSubSpace(memJoined.Id, peer, _serializer);
                        break;
                    case MemberMessage memMess:
                        memMess.Member.Tell(memMess.Message);
                        break;
                    case ObjectMessage objMess:
                        objMess.Object.Tell(objMess.Message);
                        break;
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

        public void AddNetObj(object obj)
        {

        }

        private object GetMessage(byte[] message)
        {
            using (var stream = new MemoryStream(message))
            {
                return _serializer.Deserialize(stream);
            }
        }
    }

    public sealed class NetMemberSurrogate
    {
        public Guid Id;

        public NetMemberSurrogate(Guid id)
        {
            Id = id;
        }
    }

    public sealed class NetObjSurrogate
    {
        public Guid Id;
        public ISubSpace Member;

        public NetObjSurrogate(Guid id, ISubSpace member)
        {
            Id = id;
            Member = member;
        }
    }
}
