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

        bool _renderController = true;
        LiteNetLibNetwork _controllerNet;
        SimultaneousSim _controllerSim;
        PlayerEntity _controllerPlayer;

        bool _renderAuthority = true;
        LiteNetLibNetwork _authorityNet;
        SimultaneousSim _authoritySim;
        public PlayerEntity _authorityPlayer;

        bool _renderObserver = true;
        LiteNetLibNetwork _observerNet;
        SimultaneousSim _observerSim;
        PlayerEntity _observerPlayer;

        public KeyboardDevice Keyboard;

        public Game() 
            : base("YOUR_ORGANIZATION", "PROJECT_NAME")
        {
            _frameWatch = Stopwatch.StartNew();

            _authorityNet = new LiteNetLibNetwork(CreateSerializer());
            _authoritySim = new SimultaneousSim(this, _authorityNet);
            _authoritySim.ListenToPort(5555);
            _authoritySim.ClientSimConnected += _authoritySim_ClientSimConnected;

            _controllerNet = new LiteNetLibNetwork(CreateSerializer());
            _controllerSim = new SimultaneousSim(this, _controllerNet);
            _controllerSim.ConnectToHost("localhost", 5555);

            _observerNet = new LiteNetLibNetwork(CreateSerializer());
            _observerSim = new SimultaneousSim(this, _observerNet);
            _observerSim.ConnectToHost("localhost", 5555);
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
        int simsJoined = 0;
        private void _authoritySim_ClientSimConnected(RemoteSim obj)
        {
            simsJoined++;
            if(simsJoined == 2)
            {
                _authorityPlayer = new PlayerEntity(this);
                _authoritySim.NewEntity(_authorityPlayer, _controllerSim.Id, 50);
            }
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
            Keyboard.ButtonReleased += Keyboard_ButtonReleased;

            // TODO: Load content here

            base.OnLoadingContent();
        }

        private void Keyboard_ButtonReleased(Ultraviolet.Platform.IUltravioletWindow window, KeyboardDevice device, Scancode scancode)
        {
            if(scancode == Scancode.O)
            {
                _renderObserver = !_renderObserver;
            }
            if(scancode == Scancode.C)
            {
                _renderController = !_renderController;
            }
            if(scancode == Scancode.A)
            {
                _renderAuthority = !_renderAuthority;
            }

        }

        protected override void OnUpdating(UltravioletTime time)
        {
            _lastFrameTime = _frameWatch.Elapsed.TotalMilliseconds;
            _frameWatch.Restart();

            _controllerSim.Update();
            _authoritySim.Update();
            _observerSim.Update();

            //if (_controllerPlayer != null)
            //{
            //    Console.WriteLine($"C- D: {_lastFrameTime}; S: {_controllerSim.GetTimestamp()}; P: X: {_controllerPlayer.Position.X}, Y: {_controllerPlayer.Position.Y}");
            //}

            //if (_authorityPlayer != null)
            //{
            //    Console.WriteLine($"A- D: {_lastFrameTime}; S: {_authoritySim.GetTimestamp()}; P: X: {_authorityPlayer.Position.X}, Y: {_authorityPlayer.Position.Y}");
            //}

            base.OnUpdating(time);
        }
        
        protected override void OnDrawing(UltravioletTime time)
        {
            Ultraviolet.GetGraphics().Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            if (_renderController && _controllerPlayer != null)
            {
                spriteBatch.DrawCircle(_controllerPlayer.Position, 10, 9, Color.Red);
            }
            if (_renderAuthority && _authorityPlayer != null)
            {
                spriteBatch.DrawCircle(_authorityPlayer.Position, 10, 9, Color.Purple);
            }
            if (_renderObserver && _observerPlayer != null)
            {
                spriteBatch.DrawCircle(_observerPlayer.Position, 10, 9, Color.Yellow);
            }
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

        public IEntityLogic CreateEntity(SimultaneousSim sim, object info)
        {
            var entity = new PlayerEntity(this);
            if(sim == _observerSim)
            {
                _observerPlayer = entity;
            }
            else if(sim == _controllerSim)
            {
                //entity.LogState = true;
                _controllerPlayer = entity;
            }
            return entity;
        }
    }

    public class PlayerEntity : IEntityLogic
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Speed { get; set; } = 5;
        public bool LogState { get; set; }

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
            if(LogState)
            {
                Console.WriteLine($"X: {Position.X}, Y: {Position.Y}");
            }
            var keyboard = _game.Keyboard;
            return new PlayerCommand()
            {
                Left = keyboard.IsButtonDown(Scancode.Left),
                Right = keyboard.IsButtonDown(Scancode.Right),
                Up = keyboard.IsButtonDown(Scancode.Up),
                Down = keyboard.IsButtonDown(Scancode.Down)
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
                    Velocity = vel * Speed;
                }
            }
        }

        public void Simulate(float deltaTime)
        {
            //if (_game._authorityPlayer == this)
            //{
            //    Console.WriteLine("Simulating Authority");
            //}
            //Position += Velocity * (deltaTime / 1000) * 50;
            Position += Velocity;
        }

        public object CalculateDeltas(object oldSnapshot, object newSnapshot)
        {
            return newSnapshot;
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

            public override string ToString()
            {
                return $"Left: {Left}, Right: {Right}, Up: {Up}, Down: {Down}";
            }
        }
    }
}