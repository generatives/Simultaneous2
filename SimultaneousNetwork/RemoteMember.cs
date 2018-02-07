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

        public RemoteMember(Guid id, NetPeer peer, Serializer serializer)
        {
            Id = id;
            _peer = peer;
            _serializer = serializer;
            _messages = new List<object>();
        }

        public void Tell(object obj)
        {
            _messages.Add(obj);
        }

        public void Update()
        {
            foreach(var message in _messages)
            {
                _peer.Send(new object[] { message }, _serializer);
            }
            _messages.Clear();
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
