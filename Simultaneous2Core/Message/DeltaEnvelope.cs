using System;
using System.Collections.Generic;
using System.Text;

namespace Simultaneous2Core.Message
{
    public class DeltaEnvelope
    {
        public object Deltas { get; set; }
        public long SentTimestamp { get; set; }
    }
}
