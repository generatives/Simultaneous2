using SimultaneousNetwork.SubSpace;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimultaneousNetwork.Object
{
    public interface INetObj
    {
        Guid Id { get; }
        ISubSpace SubSpace { get; }

        IReadOnlyDictionary<string, object> Traits { get; }
        T GetTrait<T>(string name);

        void Tell(INetObj sender, object message);
    }
}
