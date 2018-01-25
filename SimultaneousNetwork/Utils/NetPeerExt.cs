using Hyperion;
using LiteNetLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SimultaneousNetwork.Utils
{
    public static class NetPeerExt
    {
        private static MemoryStream _stream;

        static NetPeerExt()
        {
            _stream = new MemoryStream();
        }

        public static void Send(this NetPeer peer, object message, Serializer serializer, SendOptions sendOptions = SendOptions.ReliableOrdered)
        {
            _stream.Seek(0, SeekOrigin.Begin);
            serializer.Serialize(message, _stream);
            peer.Send(_stream.ToArray(), sendOptions);
        }
    }
}
