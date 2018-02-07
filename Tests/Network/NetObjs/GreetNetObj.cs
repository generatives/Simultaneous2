using SimultaneousNetwork;
using SimultaneousNetwork.Object;
using SimultaneousNetwork.SubSpace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.Network
{
    public class GreetNetObj : NetObj
    {
        private Query _echoQuery;

        public GreetNetObj(Guid id, ISubSpace space) : base(id, space) { }

        public override void DeclareTraits(Dictionary<string, object> traits)
        {
            traits["greet"] = true;
        }

        public override void Begin()
        {
            _echoQuery = SubSpace.Space.AddQuery(this, EchoFinder);
        }

        private bool EchoFinder(INetObj obj)
        {
            return obj.GetTrait<bool>("echo");
        }

        protected override void RecieveMessage(INetObj sender, object message)
        {
            switch(message)
            {
                case Query.Added added:
                    added.Change.Tell(this, "Hello");
                    break;
                case string echo:
                    break;
            }
        }
    }
}
