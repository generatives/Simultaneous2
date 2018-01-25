using Hyperion;
using LiteNetLib;
using SimultaneousNetwork.Object;
using SimultaneousNetwork.Object.Proxy;
using SimultaneousNetwork.SubSpace.Messages;
using SimultaneousNetwork.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimultaneousNetwork.SubSpace
{
    public class RemoteSubSpace : ISubSpace
    {
        public Guid Id { get; private set; }

        private List<object> _messages;
        private NetPeer _peer;
        private Serializer _serializer;

        public Dictionary<Guid, NetObjProxy> _proxies;

        public RemoteSubSpace(Guid id, NetPeer peer, Serializer serializer)
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
            _peer.Send(_messages.ToArray(), _serializer);
        }

        public INetObj GetObj(Guid id)
        {
            return _proxies[id];
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
