using Castle.DynamicProxy;
using SimultaneousNetwork.SubSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SimultaneousNetwork.Object.Proxy
{
    public class ObjectProxyFactory
    {
        private static ProxyGenerator proxyGenerator;

        static ObjectProxyFactory()
        {
            proxyGenerator = new ProxyGenerator();
        }

        public static object GetProxy(Type interfaceType, NetObjDescription desc)
        {
            var proxy = new NetObjProxy(desc);
            return proxyGenerator.CreateInterfaceProxyWithoutTarget(interfaceType, proxy);
        }

        public static IEnumerable<MethodInfo> GetMethods(Type t)
        {
            return t
                .GetMethods()
                .OrderBy(m => m.Name + string.Join("", m.GetParameters().Select(p => p.ParameterType.FullName)));
        }
    }
}
