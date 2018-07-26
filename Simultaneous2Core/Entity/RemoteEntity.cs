﻿using System;
using System.Collections.Generic;
using System.Text;
using Simultaneous2Core.Message;
using Simultaneous2Core.Simulation;

namespace Simultaneous2Core.Entity
{
    public class RemoteEntity : IEntity
    {
        public Guid Id { get; private set; }
        public EntityRole Role { get; private set; }

        private RemoteSim _sim;

        public RemoteEntity(Guid id, EntityRole role, RemoteSim sim)
        {
            Id = id;
            Role = role;
            _sim = sim;
        }
        
        public void SendDeltaEnvelope(DeltaEnvelope deltaEnv)
        {
            _sim.SendMessage(new EntityMessage<DeltaEnvelope>(Id, deltaEnv));
        }

        public void SendFrameRecord(FrameRecord envelope)
        {
            _sim.SendMessage(new EntityMessage<FrameRecord>(Id, envelope));
        }
    }
}