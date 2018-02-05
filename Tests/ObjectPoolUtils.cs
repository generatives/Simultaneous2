using SimultaneousNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests
{
    public static class ObjectPoolUtils
    {
        public static bool KnowsOfAll(this NetworkMember pool, params NetworkMember[] otherPools)
        {
            var ids = otherPools.Select(p => p.LocalSpace.MemberId);
            return ids.All(id => pool.RemoteMemberIds.Values.Any(p => p.Id == id));
        }
        public static bool KnowsOfOnly(this NetworkMember pool, params NetworkMember[] otherPools)
        {
            var ids = otherPools.Select(p => p.LocalSpace.MemberId);
            return pool.RemoteMemberIds.Values.All(p => ids.Contains(p.Id));
        }
    }
}
