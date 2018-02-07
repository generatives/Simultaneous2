using SimultaneousNetwork;
using SimultaneousNetwork.Object;
using SimultaneousNetwork.SubSpace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.Network
{
    public class DummyNetObj : NetObj
    {
        public DummyNetObj(Guid id, ISubSpace space) : base(id, space) { }

        public override void DeclareTraits(Dictionary<string, object> traits)
        {
            traits["test"] = true;
        }

        protected override void RecieveMessage(INetObj sender, object message)
        {
        }
    }
}
