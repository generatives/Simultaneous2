using SimultaneousNetwork;
using SimultaneousNetwork.SubSpace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.Network
{
    public class TestNetObj : NetObj
    {
        public TestNetObj(Guid id, ISubSpace space) : base(id, space) { }

        public override void DeclareTraits(Dictionary<string, object> traits)
        {
            traits["test"] = true;
        }

        protected override void RecieveMessage(object message)
        {
        }
    }
}
