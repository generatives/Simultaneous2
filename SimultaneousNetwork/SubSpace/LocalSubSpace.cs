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
    internal class LocalSubSpace : ISubSpace
    {
        public Guid MemberId { get; internal set; }

        public IEnumerable<INetObj> NetObjs => _objects.Values;

        public ObjectSpace Space { get; private set; }

        public INetObj this[Guid id] => _objects[id];
        
        private Dictionary<Guid, NetObj> _objects;

        public LocalSubSpace(ObjectSpace space)
        {
            Space = space;
            _objects = new Dictionary<Guid, NetObj>();
        }

        public void Tell(object obj)
        {

        }

        public void AddNetObj(NetObj obj)
        {
            _objects[obj.Id] = obj;
            obj.Begin();
        }

        public bool RemoveNetObj(Guid id)
        {
            if(_objects.ContainsKey(id))
            {
                var obj = _objects[id];
                _objects.Remove(id);
                obj.End();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
