using Castle.DynamicProxy;
using SimultaneousNetwork.SubSpace;
using SimultaneousNetwork.SubSpace.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SimultaneousNetwork.Object.Proxy
{
    public class NetObjProxy : INetObj, IInterceptor
    {
        public Guid Id { get; private set; }

        public ISubSpace Member { get; private set; }

        private Dictionary<MethodInfo, int> _funcs;

        public NetObjProxy(NetObjDescription desc)
        {
            Id = desc.Id;
            Member = desc.SubSpace;
            _funcs = ObjectProxyFactory.GetMethods(desc.Type)
                .ToArray()
                .Select((m, i) => new { m, i })
                .ToDictionary(tup => tup.m, tup => tup.i);
        }

        public void Tell(object message)
        {
            Member.Tell(new ObjectMessage() { Object = this, Message = message });
        }

        public void Intercept(IInvocation invocation)
        {
            var index = _funcs[invocation.Method];
            var para = invocation.Arguments;
            Tell(new FunctionCall() { FuncIndex = index, Params = para });
        }
    }
}
