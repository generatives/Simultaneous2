using SimultaneousNetwork.SubSpace;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimultaneousNetwork.Object.Proxy
{
    public class NetObjDescription
    {
        public Guid Id;
        public Guid MemberId;
        public IReadOnlyDictionary<string, object> Traits;
    }
}
