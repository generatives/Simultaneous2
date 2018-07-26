using System;
using System.Collections.Generic;
using System.Text;

namespace Simultaneous2Core.Entity
{
    public interface IEntityLogic
    {
        object GenerateCommands();
        void ProcessCommands(IEnumerable<object> commands);
        void Simulate(float delta);

        void ApplyDeltas(object deltas);
        object TakeDeltas();
        void ApplySnapshot(object snapshot);
        object TakeSnapshot();
        object GetCreationInfo();
    }
}
