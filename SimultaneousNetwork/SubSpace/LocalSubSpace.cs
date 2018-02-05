using Hyperion;
using LiteNetLib;
using LiteNetLib.Utils;
using SimultaneousNetwork.Object;
using SimultaneousNetwork.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SimultaneousNetwork.SubSpace
{
    public class LocalSubSpace : ISubSpace
    {
        public Guid MemberId => _member.Id;

        public IEnumerable<INetObj> NetObjs => _objects.Values;

        public INetObj this[Guid id] => _objects[id];

        private NetworkMember _member;
        private Dictionary<Guid, NetObj> _objects;

        public LocalSubSpace(NetworkMember member)
        {
            _member = member;
            _objects = new Dictionary<Guid, NetObj>();
        }

        public void Tell(object obj)
        {

        }

        public void AddNetObj(NetObj obj)
        {
            _objects[obj.Id] = obj;
        }

        public bool RemoveNetObj(Guid id)
        {
            if(_objects.ContainsKey(id))
            {
                _objects.Remove(id);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
