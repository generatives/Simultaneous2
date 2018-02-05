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

        public ISubSpace Member { get; private set; }

        private Dictionary<string, object> _traits;

        public NetObjProxy(NetObjDescription desc, ISubSpace space)
        {
            Id = desc.Id;
            Member = space;
            _traits = desc.Traits;
        }

        public void Tell(INetObj sender, object message)
        {
            Member.Tell(new ObjectMessage() { Object = this, Sender = sender, Message = message });
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
    }
}
