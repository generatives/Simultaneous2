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

        private MethodInfo[] _funcs;

        public NetObj(Guid id, ISubSpace member)
        {
            Id = id;
            Member = member;
            _funcs = ObjectProxyFactory.GetMethods(GetType()).ToArray();
        }

        public void Tell(object message)
        {
            switch(message)
            {
                case FunctionCall funcCall:
                    _funcs[funcCall.FuncIndex].Invoke(this, funcCall.Params);
                    break;
            }
        }
    }
}
