using SimultaneousNetwork.Object;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimultaneousNetwork.SubSpace
{
    public interface ISubSpace
    {
        Guid MemberId { get; }
        void Tell(object obj);
        INetObj this[Guid id] { get; }
        IEnumerable<INetObj> NetObjs { get; }
    }
}
