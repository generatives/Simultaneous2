using SimultaneousNetwork.Object;
using SimultaneousNetwork.Object.Proxy;
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
        public INetObj Sender;
        public object Message;
    }

    public sealed class ObjectAdded
    {
        public NetObjDescription Description;
    }

    public sealed class ObjectRemoved
    {
        public INetObj Obj;
    }

    public sealed class NetMemberSurrogate
    {
        public Guid Id;

        public NetMemberSurrogate(Guid id)
        {
            Id = id;
        }
    }

    public sealed class NetObjSurrogate
    {
        public Guid Id;
        public ISubSpace Member;

        public NetObjSurrogate(Guid id, ISubSpace member)
        {
            Id = id;
            Member = member;
        }
    }
}
