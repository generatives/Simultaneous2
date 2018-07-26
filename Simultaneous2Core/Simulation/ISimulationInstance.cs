using Simultaneous2Core.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simultaneous2Core.Simulation
{
    public interface ISimulationInstance
    {
        InstanceRole Role { get; }
    }

    public enum InstanceRole
    {
        OWNER, CLIENT
    }
}
