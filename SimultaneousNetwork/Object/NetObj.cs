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
    public class NetObj : INetObj
    {
        public Guid Id { get; private set; }
        public ISubSpace SubSpace { get; private set; }

        private Dictionary<string, object> _traits;
        public IReadOnlyDictionary<string, object> Traits => _traits;

        public NetObj(Guid id, ISubSpace subSpace)
        {
            Id = id;
            SubSpace = subSpace;
            _traits = new Dictionary<string, object>();
            DeclareTraits(_traits);
        }

        public void Tell(INetObj sender, object message)
        {
            RecieveMessage(sender, message);
        }

        public T GetTrait<T>(string name)
        {
            if (_traits.ContainsKey(name))
            {
                return (T)_traits[name];
            }
            else
            {
                return default(T);
            }
        }

        public virtual void DeclareTraits(Dictionary<string, object> traits)
        {

        }

        public virtual void Begin()
        {

        }

        protected virtual void RecieveMessage(INetObj sender, object message)
        {

        }

        public virtual void End()
        {

        }
    }
}
