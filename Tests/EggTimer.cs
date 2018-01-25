using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    public class EggTimer
    {
        public static void In(int milli, Action action)
        {
            Task.Run(() =>
            {
                Thread.Sleep(milli);
                action();
            });
        }

        public static void Until(int maxTime, Action action)
        {
            bool done = false;
            In(maxTime, () => done = true);
            while (!done)
            {
                action();
            }
        }

        public static void Until(int maxTime, Func<bool> action)
        {
            bool done = false;
            In(maxTime, () => done = true);
            while (!done && !action()) { }
        }
    }
}
