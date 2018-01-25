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
        public Guid Id { get; private set; }

        private Dictionary<Guid, NetObj> _objects;

        public LocalSubSpace(Guid id)
        {
            Id = id;
            _objects = new Dictionary<Guid, NetObj>();
        }

        public void Tell(object obj)
        {

        }

        public INetObj GetObj(Guid id)
        {
            return _objects[id];
        }

        public void AddNetObj(NetObj obj)
        {

        }
    }
}
