using SimultaneousNetwork;
using SimultaneousNetwork.Object;
using SimultaneousNetwork.SubSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests.Network
{
    public class Utils
    {
        public static List<ObjectSpace> StartLocalNetwork(int port, int number)
        {
            List<ObjectSpace> members = new List<ObjectSpace>(number);
            var host = ObjectSpace.Start(port);
            members.Add(host);
            EggTimer.Until(10, () => host.Update());

            while(members.Count < number)
            {
                var newClient = ObjectSpace.Join("localhost", port);
                members.Add(newClient);
                EggTimer.Until(500, () =>
                {
                    members.ForEach(mem => mem.Update());
                    return newClient.State == MemberState.JOINED &&
                        members.All(mem => mem.SubSpaces.Count() == members.Count);
                });
            }

            return members;
        }

        public static void AddNetObj(ObjectSpace space, Func<Guid, ISubSpace, NetObj> func, List<ObjectSpace> members)
        {
            var counts = members.Select(s => s.NetObjs.Count());
            var start = counts.First();
            foreach(var num in counts)
            {
                if(num != start)
                {
                    throw new Exception("All cluster members should start with equal NetObj counts");
                }
            }
            space.AddNetObj(func);
            EggTimer.Until(500, () =>
            {
                members.ForEach(mem => mem.Update());
                return members.All(mem => mem.NetObjs.Count() == start + 1);
            });
        }
    }
}
