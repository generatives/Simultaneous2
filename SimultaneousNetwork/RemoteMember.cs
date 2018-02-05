using Hyperion;
using LiteNetLib;
using SimultaneousNetwork.SubSpace;
using SimultaneousNetwork.SubSpace.Messages;
using SimultaneousNetwork.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimultaneousNetwork
{
    public class RemoteMember
    {
        public Guid Id { get; private set; }

        private List<object> _messages;
        private NetPeer _peer;
        private Serializer _serializer;

        public RemoteSubSpace Space { get; private set; }

        public RemoteMember(Guid id, NetPeer peer, Serializer serializer)
        {
            Id = id;
            _peer = peer;
            _serializer = serializer;
            _messages = new List<object>();
            Space = new RemoteSubSpace(this);
        }

        public void Tell(object obj)
        {
            _messages.Add(obj);
        }

        public void Update()
        {
            if(_messages.Count > 0)
            {
                _peer.Send(_messages.ToArray(), _serializer);
                _messages.Clear();
            }
        }

        public NetMemberRef GetRef()
        {
            return new NetMemberRef()
            {
                Id = Id,
                Address = _peer.EndPoint.Host,
                Port = _peer.EndPoint.Port
            };
        }
    }
}
