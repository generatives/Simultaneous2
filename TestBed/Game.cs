using Hyperion;
using Primitives2D;
using Simultaneous2Core;
using Simultaneous2Core.Entity;
using Simultaneous2Core.Simulation;
using SimultaneousLiteNetLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ultraviolet;
using Ultraviolet.Content;
using Ultraviolet.Core;
using Ultraviolet.Graphics.Graphics2D;
using Ultraviolet.Input;
using Ultraviolet.OpenGL;

namespace TestBed
{
    public class Game : UltravioletApplication, ISimultaneousInterface
    {
        Stopwatch _frameWatch;
        double _lastFrameTime;

        SpriteBatch spriteBatch;

        LiteNetLibNetwork _clientNet;
        SimultaneousSim _clientSim;
        PlayerEntity _clientPlayer;


        LiteNetLibNetwork _authorityNet;
        SimultaneousSim _authoritySim;
        PlayerEntity _authorityPlayer;

        public KeyboardDevice Keyboard;

        public Game() 
            : base("YOUR_ORGANIZATION", "PROJECT_NAME")
        {
            _frameWatch = Stopwatch.StartNew();

            _authorityNet = new LiteNetLibNetwork(CreateSerializer());
            _authoritySim = new SimultaneousSim(this, _authorityNet);
            _authoritySim.ListenToPort(5555);
            _authoritySim.ClientSimConnected += _authoritySim_ClientSimConnected;

            _clientNet = new LiteNetLibNetwork(CreateSerializer());
            _clientSim = new SimultaneousSim(this, _clientNet);
            _clientSim.ConnectToHost("localhost", 5555);
        }

        private Serializer CreateSerializer()
        {
            return new Serializer(new SerializerOptions(
                false,
                true,
                null,
                null,
                null,
                false));
        }

        private void _authoritySim_ClientSimConnected(RemoteSim obj)
        {
            _authorityPlayer = new PlayerEntity(this);
            _authoritySim.NewEntity(_authorityPlayer, _clientSim.Id, 50);
        }

        protected override UltravioletContext OnCreatingUltravioletContext()
        {
            var configuration = new OpenGLUltravioletConfiguration();
            PopulateConfiguration(configuration);

#if DEBUG
            configuration.Debug = true;
            configuration.DebugLevels = DebugLevels.Error | DebugLevels.Warning;
            configuration.DebugCallback = (uv, level, message) =>
            {
                Console.WriteLine(message);
            };
#endif

            return new OpenGLUltravioletContext(this, configuration);
        }

        protected override void OnLoadingContent()
        {
            this.content = ContentManager.Create("Content");
            spriteBatch = SpriteBatch.Create();
            Keyboard = Ultraviolet.GetInput().GetKeyboard();

            // TODO: Load content here

            base.OnLoadingContent();
        }

        protected override void OnUpdating(UltravioletTime time)
        {
            _lastFrameTime = _frameWatch.Elapsed.TotalMilliseconds;
            _frameWatch.Restart();

            _clientSim.Update();
            _authoritySim.Update();

            base.OnUpdating(time);
        }
        
        protected override void OnDrawing(UltravioletTime time)
        {
            Ultraviolet.GetGraphics().Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            if (_clientPlayer != null)
            {
                spriteBatch.DrawCircle(_clientPlayer.Position, 10, 9, Color.Red);
            }
            //if (_authorityPlayer != null)
            //{
            //    spriteBatch.DrawCircle(_authorityPlayer.Position, 10, 9, Color.Purple);
            //}
            spriteBatch.End();

            base.OnDrawing(time);
        }
        
        protected override void Dispose(Boolean disposing)
        {
            if (disposing)
            {
                SafeDispose.DisposeRef(ref content);
            }
            base.Dispose(disposing);
        }

        private ContentManager content;

        public double GetDeltaTime()
        {
            return _lastFrameTime;
        }

        public IEntityLogic CreateEntity(object info)
        {
            if (_clientPlayer != null) throw new Exception("Whaaaaat");
            _clientPlayer = new PlayerEntity(this);
            return _clientPlayer;
        }
    }

    public class PlayerEntity : IEntityLogic
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public LocalEntity Entity { get; set; }

        private Game _game;

        public PlayerEntity(Game game)
        {
            _game = game;
        }

        public void ApplyDeltas(object deltas)
        {
            var state = (PlayerState)deltas;
            Position = state.Position;
            Velocity = state.Velocity;
        }

        public void ApplySnapshot(object snapshot)
        {
            var state = (PlayerState)snapshot;
            Position = state.Position;
            Velocity = state.Velocity;
        }

        public object GenerateCommands()
        {
            //Console.WriteLine($"X: {Position.X}, Y: {Position.Y}");
            var keyboard = _game.Keyboard;
            return new PlayerCommand()
            {
                Left = keyboard.IsButtonDown(Scancode.A),
                Right = keyboard.IsButtonDown(Scancode.D),
                Up = keyboard.IsButtonDown(Scancode.W),
                Down = keyboard.IsButtonDown(Scancode.S)
            };
            //return new PlayerCommand()
            //{
            //    Left = false,
            //    Right = true,
            //    Up = false,
            //    Down = true
            //};
        }

        public object GetCreationInfo()
        {
            return 0;
        }

        public void ProcessCommands(IEnumerable<object> commands)
        {
            foreach (var command in commands)
            {
                if (command is PlayerCommand pCommand)
                {
                    var vel = new Vector2();
                    if (pCommand.Left)
                    {
                        vel += new Vector2(-1, 0);
                    }
                    if (pCommand.Right)
                    {
                        vel += new Vector2(1, 0);
                    }
                    if (pCommand.Up)
                    {
                        vel += new Vector2(0, -1);
                    }
                    if (pCommand.Down)
                    {
                        vel += new Vector2(0, 1);
                    }
                    Velocity = vel;
                }
            }
        }

        public void Simulate(float delta)
        {
            Position += Velocity * (delta / 1000) * 50;
        }

        public object TakeDeltas()
        {
            return new PlayerState() { Position = Position, Velocity = Velocity };
        }

        public object TakeSnapshot()
        {
            return new PlayerState() { Position = Position, Velocity = Velocity };
        }

        public class PlayerState
        {
            public Vector2 Position { get; set; }
            public Vector2 Velocity { get; set; }

            public override string ToString()
            {
                return $"Px: {Position.X}, Py: {Position.Y}, Vx: {Velocity.X}, Vy: {Velocity.Y}";
            }
        }

        public class PlayerCommand
        {
            public bool Left, Right, Up, Down;
        }
    }
}