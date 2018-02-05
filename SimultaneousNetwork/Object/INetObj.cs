using SimultaneousNetwork.SubSpace;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimultaneousNetwork.Object
{
    public interface INetObj
    {
        Guid Id { get; }
        ISubSpace Member { get; }

        object GetTrait(string name);

        void Tell(INetObj sender, object message);
    }
}
