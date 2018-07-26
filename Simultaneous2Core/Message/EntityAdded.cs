using System;
using System.Collections.Generic;
using System.Text;

namespace Simultaneous2Core.Message
{
    public class EntityAdded
    {
        public Guid EntityId { get; set; }
        public object CreationInfo { get; set; }
        public object InitialSnapshot { get; set; }
        public float UpdatePeriod { get; set; }
        public Guid AuthoritySimId { get; set; }
        public Guid ControllerSimId { get; set; }
    }
}
