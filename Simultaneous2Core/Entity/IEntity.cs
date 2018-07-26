using Simultaneous2Core.Message;
using System;

namespace Simultaneous2Core.Entity
{
    public interface IEntity
    {
        Guid Id { get; }
        EntityRole Role { get; }
        void SendFrameRecord(FrameRecord envelope);
        void SendDeltaEnvelope(DeltaEnvelope deltaEnv);
    }

    [Flags]
    public enum EntityRole
    {
        EMPTY = 0,
        AUTHORITY = 1,
        CONTROLLER = 2,
        OBSERVER = 4
    }

    public static class EntityRoleExt
    {
        public static bool IsInRole(this EntityRole role, EntityRole otherRole)
        {
            return (role & otherRole) == otherRole;
        }
    }
}