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
        static ObjectProxyFactory()
        {
        }

        public static object GetProxy(Type interfaceType, NetObjDescription desc)
        {
            return new object();
        }
    }
}
