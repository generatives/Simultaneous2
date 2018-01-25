using SimultaneousNetwork.Object;
using SimultaneousNetwork.SubSpace;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimultaneousNetwork.SubSpace.Messages
{
    public sealed class NetMemberRef
    {
        public Guid Id;
        public string Address;
        public int Port;
    }

    public sealed class RequestOnBoarding
    {

    }

    public sealed class OnBoarding
    {
        public Guid AssignedId;
        public Guid SeedId;
        public NetMemberRef[] Members;
    }

    public sealed class FinishedOnBoarding
    {

    }

    public sealed class MemberJoined
    {
        public Guid Id;
    }

    public sealed class MemberMessage
    {
        public ISubSpace Member;
        public object Message;
    }

    public sealed class ObjectMessage
    {
        public INetObj Object;
        public object Message;
    }

    public sealed class FunctionCall
    {
        public int FuncIndex;
        public object[] Params;
    }
}
