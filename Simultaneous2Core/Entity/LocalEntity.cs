using Simultaneous2Core.Message;
using Simultaneous2Core.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simultaneous2Core.Entity
{
    public class LocalEntity : IEntity
    {
        public Guid Id { get; private set; }
        public EntityRole Role { get; private set; }

        private SimultaneousSim _sim;

        private IEntity _authorityEntity;
        private IEntity _controllerEntity;
        private List<IEntity> _clientEntities;

        private IEntityLogic _logic;

        private List<FrameRecord> _recordedFrames;
        private List<RecievedCmdEnvelope> _recievedEnvelopes;
        private object _lastValidSnapshot;

        public float UpdatePeriod { get; set; }
        public float _timeSinceLastUpdate;

        public LocalEntity(Guid id, IEntityLogic logic, SimultaneousSim sim, EntityRole role, float updatePeriod = 100f)
        {
            Id = id;
            _logic = logic;
            _sim = sim;
            Role = role;

            _recordedFrames = new List<FrameRecord>();
            _recievedEnvelopes = new List<RecievedCmdEnvelope>();

            _clientEntities = new List<IEntity>();

            UpdatePeriod = updatePeriod;

            NetworkEntityAdded(this);
        }

        public void RecieveEnvelope(FrameRecord envelope)
        {
            _recievedEnvelopes.Add(new RecievedCmdEnvelope()
            {
                Envelope = envelope,
                RecievedTimestamp = _sim.GetTimestamp()
            });
        }

        public void RecieveDeltas(DeltaEnvelope deltaEnv)
        {
            if(!Role.IsInRole(EntityRole.AUTHORITY))
            {
                if (_lastValidSnapshot != null)
                {
                    _logic.ApplySnapshot(_lastValidSnapshot);
                }
                //Console.WriteLine($"State Before Delta: {_logic.TakeSnapshot()}");
                //Console.WriteLine($"Applying Delta: {deltaEnv.Deltas} From: {deltaEnv.SentTimestamp} It Is: {_sim.GetTimestamp()}");
                _logic.ApplyDeltas(deltaEnv.Deltas);

                _lastValidSnapshot = _logic.TakeSnapshot();

                _recordedFrames = _recordedFrames
                    .Where(e => e.SentTimestamp > deltaEnv.SentTimestamp)
                    .ToList();
                var processCommands = Role.IsInRole(EntityRole.CONTROLLER);
                foreach (var env in _recordedFrames)
                {
                    if(processCommands)
                    {
                        _logic.ProcessCommands(env.Commands);
                    }
                    _logic.Simulate(env.SentDelta);
                }

                //Console.WriteLine($"State After Simulation: {_logic.TakeSnapshot()}");
            }
        }

        public void NetworkEntityAdded(IEntity entity)
        {
            if(entity.Role.IsInRole(EntityRole.AUTHORITY))
            {
                _authorityEntity = entity;
            }
            if (entity.Role.IsInRole(EntityRole.CONTROLLER))
            {
                _controllerEntity = entity;
            }
            if (entity.Role.IsInRole(EntityRole.OBSERVER))
            {
                _clientEntities.Add(entity);
            }
        }

        public void Update()
        {
            var delta = _sim.GetDeltaTime();
            var currentFrame = new FrameRecord()
            {
                Commands = new List<object>(),
                SentDelta = delta,
                SentTimestamp = _sim.GetTimestamp()
            };

            _recordedFrames.Add(currentFrame);

            if (Role.IsInRole(EntityRole.CONTROLLER))
            {
                var commands = _logic.GenerateCommands();
                currentFrame.Commands.Add(commands);
                if(!Role.IsInRole(EntityRole.AUTHORITY))
                {
                    _logic.ProcessCommands(currentFrame.Commands);
                }

                _authorityEntity.SendFrameRecord(currentFrame);
            }

            if (Role.IsInRole(EntityRole.AUTHORITY))
            {
                _logic.ProcessCommands(_recievedEnvelopes.SelectMany(e => e.Envelope.Commands));
                _recievedEnvelopes.Clear();
            }

            _logic.Simulate(delta);

            if(Role.IsInRole(EntityRole.AUTHORITY))
            {
                _timeSinceLastUpdate += delta;
                if (_timeSinceLastUpdate > UpdatePeriod)
                {
                    _timeSinceLastUpdate = 0;
                    var timestamp = _sim.GetTimestamp();
                    var deltas = _logic.TakeDeltas();
                    var env = new DeltaEnvelope()
                    {
                        Deltas = deltas,
                        SentTimestamp = timestamp
                    };
                    foreach (var client in _clientEntities)
                    {
                        client.SendDeltaEnvelope(env);
                    }
                    _controllerEntity?.SendDeltaEnvelope(env);
                }
            }
        }

        public void SendFrameRecord(FrameRecord envelope)
        {
            RecieveEnvelope(envelope);
        }

        public void SendDeltaEnvelope(DeltaEnvelope deltaEnv)
        {
            RecieveDeltas(deltaEnv);
        }
    }
}
