using Simultaneous2Core.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simultaneous2Core
{
    public interface ISimultaneousInterface
    {
        double GetDeltaTime();
        IEntityLogic CreateEntity(object info);
    }
}
