using Simultaneous2Core.Entity;
using Simultaneous2Core.Simulation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simultaneous2Core
{
    public interface ISimultaneousInterface
    {
        double GetDeltaTime();
        IEntityLogic CreateEntity(SimultaneousSim sim, object info);
    }
}
