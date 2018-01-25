using SimultaneousNetwork.Object;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimultaneousNetwork.SubSpace
{
    public interface ISubSpace
    {
        Guid Id { get; }
        void Tell(object obj);
        INetObj GetObj(Guid id);
    }
}
