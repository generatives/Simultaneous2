using SimultaneousNetwork.Object;
using SimultaneousNetwork.Object.Proxy;
using SimultaneousNetwork.SubSpace;
using SimultaneousNetwork.SubSpace.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SimultaneousNetwork
{
    public abstract class NetObj : INetObj
    {
        public Guid Id { get; private set; }
        public ISubSpace Member { get; private set; }

        private Dictionary<string, object> _traits;
        internal Dictionary<string, object> Traits => _traits;

        public NetObj(Guid id, ISubSpace member)
        {
            Id = id;
            Member = member;
            _traits = new Dictionary<string, object>();
            DeclareTraits(_traits);
        }

        public void Tell(INetObj sender, object message)
        {
            RecieveMessage(sender, message);
        }

        public object GetTrait(string name)
        {
            if (_traits.ContainsKey(name))
            {
                return _traits[name];
            }
            else
            {
                return null;
            }
        }

        public abstract void DeclareTraits(Dictionary<string, object> traits);
        protected abstract void RecieveMessage(INetObj sender, object message);
    }
}
