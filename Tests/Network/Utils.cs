using SimultaneousNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests.Network
{
    public class Utils
    {
        public static NetworkMember[] StartLocalNetwork(int port, int number)
        {
            List<NetworkMember> members = new List<NetworkMember>(number);
            var host = NetworkMember.Start(port);
            members.Add(host);
            EggTimer.Until(10, () => host.Update());

            while(members.Count < number)
            {
                var newClient = NetworkMember.Join("localhost", port);
                members.Add(newClient);
                EggTimer.Until(500, () =>
                {
                    members.ForEach(mem => mem.Update());
                    return newClient.State == MemberState.JOINED &&
                        members.All(mem => mem.RemoteMemberIds.Count == members.Count - 1);
                });
            }

            return members.ToArray();
        }
    }
}
