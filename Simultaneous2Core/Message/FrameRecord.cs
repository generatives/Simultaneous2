using System;
using System.Collections.Generic;
using System.Text;

namespace Simultaneous2Core.Message
{
    public class FrameRecord
    {
        public List<object> Commands { get; set; }
        public long SentTimestamp { get; set; }
        public float SentDelta { get; set; }
    }

    public class RecievedCmdEnvelope
    {
        public FrameRecord Envelope { get; set; }
        public long RecievedTimestamp { get; set; }
        public bool HasBeenProcessed { get; set; }
    }
}
