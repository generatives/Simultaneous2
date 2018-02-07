using SimultaneousNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests
{
    public static class ObjectPoolUtils
    {
        public static bool KnowsOfAll(this ObjectSpace pool, params ObjectSpace[] otherPools)
        {
            return pool.NetworkMember.KnowsOfAll(otherPools.Select(p => p.NetworkMember).ToArray());
        }
        public static bool KnowsOfOnly(this ObjectSpace pool, params ObjectSpace[] otherPools)
        {
            return pool.NetworkMember.KnowsOfOnly(otherPools.Select(p => p.NetworkMember).ToArray());
        }

        public static bool KnowsOfAll(this NetworkMember pool, params NetworkMember[] otherPools)
        {
            var ids = otherPools.Select(p => p.Id);
            return ids.All(id => pool.RemoteMemberIds.Values.Any(p => p.Id == id));
        }
        public static bool KnowsOfOnly(this NetworkMember pool, params NetworkMember[] otherPools)
        {
            var ids = otherPools.Select(p => p.Id);
            return pool.RemoteMemberIds.Values.All(p => ids.Contains(p.Id));
        }
    }
}
