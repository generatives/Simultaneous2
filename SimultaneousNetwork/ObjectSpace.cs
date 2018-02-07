using SimultaneousNetwork.Object;
using SimultaneousNetwork.Object.Proxy;
using SimultaneousNetwork.SubSpace;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimultaneousNetwork
{
    public class ObjectSpace
    {
        private LocalSubSpace _localSpace;
        private Dictionary<Guid, RemoteSubSpace> _remoteSpaces;

        private List<Query> _queries;

        public NetworkMember NetworkMember { get; private set; }

        #region NetworkMember Pass Throughs
        public MemberState State => NetworkMember.State;
        #endregion

        public IEnumerable<ISubSpace> SubSpaces
        {
            get
            {
                foreach (var remote in _remoteSpaces.Values)
                {
                    yield return remote;
                }
                if (_localSpace != null)
                {
                    yield return _localSpace;
                }
            }
        }

        public IEnumerable<INetObj> NetObjs
        {
            get
            {
                foreach(var space in SubSpaces)
                {
                    foreach(var obj in space.NetObjs)
                    {
                        yield return obj;
                    }
                }
            }
        }

        #region Constructor Wrappers
        public static ObjectSpace Start(int port, int maxMembers = 100, string connectionKey = "SimultaneousNetwork")
        {
            var space = new ObjectSpace(port, maxMembers, connectionKey);
            return space;
        }

        public static ObjectSpace Join(string seedAddress, int seedPort, int maxMembers = 100, string connectionKey = "SimultaneousNetwork")
        {
            var space = new ObjectSpace(seedAddress, seedPort, maxMembers, connectionKey);
            return space;
        }
        #endregion

        private ObjectSpace(int port, int maxMembers = 100, string connectionKey = "SimultaneousNetwork") : this()
        {
            NetworkMember = NetworkMember.Start(this, port, maxMembers, connectionKey);
        }

        private ObjectSpace(string seedAddress, int seedPort, int maxMembers = 100, string connectionKey = "SimultaneousNetwork") : this()
        {
            NetworkMember = NetworkMember.Join(this, seedAddress, seedPort, maxMembers, connectionKey);
        }

        private ObjectSpace()
        {
            _localSpace = new LocalSubSpace(this);
            _remoteSpaces = new Dictionary<Guid, RemoteSubSpace>();

            _queries = new List<Query>();
        }

        internal void MemberIdAssigned(Guid id)
        {
            _localSpace.MemberId = id;
        }

        internal void RemoteSpaceAdded(RemoteSubSpace space)
        {
            _remoteSpaces[space.MemberId] = space;
        }

        internal void RemoteSpaceRemoved(RemoteSubSpace space)
        {
            _remoteSpaces.Remove(space.MemberId);
        }

        internal void RemoteNetObjAdded(NetObjDescription desc)
        {
            var obj = _remoteSpaces[desc.MemberId].AddObj(desc);
            AnyNetObjAdded(obj);
        }

        internal void RemoteNetObjRemoved(Guid objId, Guid memberId)
        {
            var obj = _remoteSpaces[memberId].RemoveObj(objId);
            AnyNetObjRemoved(obj);
        }

        public ISubSpace GetSubSpace(Guid id)
        {
            return (id == _localSpace.MemberId ? _localSpace as ISubSpace : _remoteSpaces[id]);
        }

        public void AddNetObj(Func<Guid, ISubSpace, NetObj> factory)
        {
            var obj = factory(Guid.NewGuid(), _localSpace);
            AnyNetObjAdded(obj);
            NetworkMember.NetObjAdded(obj);
            _localSpace.AddNetObj(obj);
        }

        public void RemoveNetObj(INetObj obj)
        {
            var removed = _localSpace.RemoveNetObj(obj.Id);
            if (removed)
            {
                AnyNetObjRemoved(obj);

                NetworkMember.NetObjRemoved(obj);
            }
        }

        public Query AddQuery(INetObj sub, Func<INetObj, bool> exp)
        {
            var query = new Query(sub, exp);
            _queries.Add(query);
            foreach(var obj in NetObjs)
            {
                query.ObjAdded(obj);
            }
            return query;
        }

        public void RemoveQuery(Query query)
        {
            _queries.Remove(query);
        }

        public void Update()
        {
            NetworkMember.Update();
        }

        private void AnyNetObjAdded(INetObj obj)
        {
            foreach (var query in _queries)
            {
                query.ObjAdded(obj);
            }
        }

        private void AnyNetObjRemoved(INetObj obj)
        {
            foreach (var query in _queries)
            {
                query.ObjRemoved(obj);
            }
        }
    }

    public class NetObjEvent : EventArgs
    {
        public INetObj Obj { get; private set; }

        public NetObjEvent(INetObj obj)
        {
            Obj = obj;
        }
    }
}
