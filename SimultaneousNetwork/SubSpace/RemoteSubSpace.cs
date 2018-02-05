using Hyperion;
using LiteNetLib;
using SimultaneousNetwork.Object;
using SimultaneousNetwork.Object.Proxy;
using SimultaneousNetwork.SubSpace.Messages;
using SimultaneousNetwork.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimultaneousNetwork.SubSpace
{
    public class RemoteSubSpace : ISubSpace
    {
        private RemoteMember _member;
        public Dictionary<Guid, NetObjProxy> _proxies;

        public Guid MemberId => _member.Id;

        public IEnumerable<INetObj> NetObjs => _proxies.Values;

        public INetObj this[Guid id] => _proxies[id];

        public RemoteSubSpace(RemoteMember member)
        {
            _proxies = new Dictionary<Guid, NetObjProxy>();
            _member = member;
        }

        public void Tell(object obj)
        {
            _member.Tell(obj);
        }

        public INetObj AddObj(NetObjDescription desc)
        {
            var obj = new NetObjProxy(desc, this);
            _proxies[desc.Id] = obj;
            return obj;
        }

        public bool RemoveObj(INetObj obj)
        {
            if(_proxies.ContainsKey(obj.Id))
            {
                _proxies.Remove(obj.Id);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
