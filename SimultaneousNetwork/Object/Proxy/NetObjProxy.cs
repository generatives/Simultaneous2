using SimultaneousNetwork.SubSpace;
using SimultaneousNetwork.SubSpace.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SimultaneousNetwork.Object.Proxy
{
    public class NetObjProxy : INetObj
    {
        public Guid Id { get; private set; }

        public ISubSpace SubSpace { get; private set; }

        public IReadOnlyDictionary<string, object> Traits { get; private set; }

        public NetObjProxy(NetObjDescription desc, ISubSpace space)
        {
            Id = desc.Id;
            SubSpace = space;
            Traits = desc.Traits;
        }

        public void Tell(INetObj sender, object message)
        {
            SubSpace.Tell(new ObjectMessage() { Object = this, Sender = sender, Message = message });
        }

        public T GetTrait<T>(string name)
        {
            if (Traits.ContainsKey(name))
            {
                return (T)Traits[name];
            }
            else
            {
                return default(T);
            }
        }
    }
}
