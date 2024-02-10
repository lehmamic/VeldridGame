using System.Diagnostics;
using Silk.NET.Maths;
using Veldrid;
using VeldridGame.Abstractions;
using VeldridGame.Camera;
using VeldridGame.GameObjects;
using VeldridGame.Input;
using VeldridGame.Maths;
using VeldridGame.Rendering;
using VeldridGame.Terrains;

namespace VeldridGame;

public class Game : IDisposable
{
    private const int FramesPerSecond = 60;
    private static readonly TimeSpan TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / FramesPerSecond);

    private readonly Renderer _renderer;
    private readonly InputSystem _inputSystem = new();
    
    // All the actors in the game
    private readonly List<Actor> _actors = new();
    private readonly List<Actor> _pendingActors = new();
    
    private bool _updatingActors = false;

    private CameraActor _cameraActor;

    public Game()
    {
        _renderer = new Renderer(this, 1024, 768, "Veldrid Game");
    }
    
    public Renderer Renderer => _renderer;
    
    public InputSystem InputSystem => _inputSystem;
    
    public GameState State { get; set; } = GameState.GamePlay;

    public void RunLoop()
    {
        LoadData();
            
    
        var gameTimer = Stopwatch.StartNew();
        var accumulatedElapsedTime = TimeSpan.Zero;

        while (State != GameState.Quit)
        {
            TimeSpan elapsedTime = gameTimer.Elapsed;
            TimeSpan deltaTime = elapsedTime - accumulatedElapsedTime;

            if (deltaTime >= TargetElapsedTime)
            {
                accumulatedElapsedTime += deltaTime;
                
                ProcessInput();
                UpdateGame((float)deltaTime.TotalSeconds);
                GenerateOutput();
            }
        }
    }
    
    public void AddActor(Actor actor)
    {
        // If updating actors, need to add to pending
        if (_updatingActors)
        {
            _pendingActors.Add(actor);
        }
        else
        {
            _actors.Add(actor);
        }
    }
    
    public void RemoveActor(Actor actor)
    {
        // Is it in pending actors?
        _pendingActors.Remove(actor);

        // Is it in actors?
        _actors.Remove(actor);
    }

    public void Dispose()
    {
        UnloadData();
        _renderer.Dispose();
    }
    
    private void ProcessInput()
    {
        var input = Renderer.Window.PumpEvents();
        _inputSystem.Update(input);

        if (State == GameState.GamePlay)
        {
            HandleKeyPress( _inputSystem.State);
        }
    
        if (State == GameState.GamePlay)
        {
            // Process input for all actors
            _updatingActors = true;
            foreach (var actor in _actors.Where(a => a.State == ActorState.Active))
            {
                actor.ProcessInput( _inputSystem.State);
            }
            _updatingActors = false;
        }
    }
    
    private void UpdateGame(float deltaTime)
    {
        // This belongs actually to the game loop, but we cant to it that way because silk.net makes the game loop as a black box...
        if (State == GameState.Quit)
        {
            _renderer.Window.Close();
        }

        if (State == GameState.GamePlay)
        {
            // Update all actors
            _updatingActors = true;
            foreach (var actor in _actors)
            {
                actor.Update(deltaTime);
            }
            _updatingActors = false;

            // Move any pending actors to _actors
            foreach (var pending in _pendingActors)
            {
                pending.Transform.ComputeWorldTransform();
                _actors.Add(pending);
            }

            _pendingActors.Clear();

            // Delete dead actors (which removes them from _actors)
            var deadActors = _actors.Where(a => a.State == ActorState.Dead).ToArray();
            foreach (var actor in deadActors)
            {
                actor.Dispose();
            }
        }
    }

    private void GenerateOutput()
    {
        _renderer.Draw();
    }
    
    private void HandleKeyPress(InputState state)
    {
        if (state.Keyboard.GetKeyState(Key.Escape) == ButtonState.Pressed)
        {
            State = GameState.Quit;
        }
    }

    private void LoadData()
    {
        // Create actors
        var actor = new Actor(this);
        actor.Transform.Position = new Vector3D<float>(200.0f, 75.0f, 0.0f);
        actor.Transform.Scale = 100.0f;

        var q = Quaternion<float>.CreateFromAxisAngle(Vector3D<float>.UnitY, -1 * Scalar<float>.PiOver2);
        q = Quaternion<float>.Concatenate(q, GameMath.CreateQuaternion(Vector3D<float>.UnitZ, (float)(Math.PI + Math.PI / 4.0f)));
        actor.Transform.Rotation = q;
        _ = new MeshComponent(actor)
        {
            Mesh = _renderer.GetMesh("Assets/Cube.gpmesh")
        };

        actor = new Actor(this);
        actor.Transform.Position = new Vector3D<float>(200.0f, -75.0f, 0.0f);
        actor.Transform.Scale = 3.0f;
        
        _ = new MeshComponent(actor)
        {
            Mesh = _renderer.GetMesh("Assets/Sphere.gpmesh")
        };
        
        // Setup floor
        actor = new Actor(this);
        actor.Transform.Position = new Vector3D<float>(0.0f, 0.0f, -100.0f);
        
        _ = new TerrainComponent(actor)
        {
            Terrain = _renderer.GetTerrain("Assets/heightmap_flat_test.png")
        };
        
        var start = -1250.0f;
        var size = 250.0f;
        // for (int i = 0; i < 10; i++)
        // {
        //     for (int j = 0; j < 10; j++)
        //     {
        //         actor = new PlaneActor(this);
        //         actor.Transform.Position = new Vector3D<float>(start + i * size, start + j * size, -100.0f);
        //     }
        // }

        // Left/right walls
        q = GameMath.CreateQuaternion(Vector3D<float>.UnitX, Scalar<float>.PiOver2);
        for (int i = 0; i < 10; i++)
        {
            actor = new PlaneActor(this);
            actor.Transform.Position = new Vector3D<float>(start + i * size, start - size, 0.0f);
            actor.Transform.Rotation = q;
        
            actor = new PlaneActor(this);
            actor.Transform.Position = new Vector3D<float>(start + i * size, -start + size, 0.0f);
            actor.Transform.Rotation = q;
        }
        
        q = Quaternion<float>.Concatenate(q, GameMath.CreateQuaternion(Vector3D<float>.UnitZ, Scalar<float>.PiOver2));
        // Forward/back walls
        for (int i = 0; i < 10; i++)
        {
            actor = new PlaneActor(this);
            actor.Transform.Position = new Vector3D<float>(start - size, start + i * size, 0.0f);
            actor.Transform.Rotation = q;
        
            actor = new PlaneActor(this);
            actor.Transform.Position = new Vector3D<float>(-start + size, start + i * size, 0.0f);
            actor.Transform.Rotation = q;
        }

        // Setup lights
        _renderer.AmbientLight = new AmbientLightInfo(
            color: new Vector3D<float>(0.2f, 0.2f, 0.2f)
        );
        _renderer.DirectionalLightInfo = new DirectionalLightInfo(
            direction: new Vector3D<float>(0.0f, -0.707f, -0.707f),
            diffuseColor: new Vector3D<float>(0.78f, 0.88f, 1.0f),
            specularColor: new Vector3D<float>(0.8f, 0.8f, 0.8f)
        );
        
        // Camera actor
        _cameraActor = new CameraActor(this);
        // _cameraActor.Transform.Position = new Vector3D<float>(0.0f, 0.0f, 100.0f);
        // _cameraActor.Transform.RotateToNewForward(new Vector3D<float>(0.0f, 0.0f, -100.0f));
        
        // UI elements
        actor = new Actor(this);
        actor.Transform.Position = new Vector3D<float>(-350.0f, -350.0f, 0.0f);
            
        _ = new SpriteComponent(actor)
        {
            Texture = _renderer.GetTexture("Assets/HealthBar.png")
        };

        actor = new Actor(this);
        actor.Transform.Position = new Vector3D<float>(375.0f, -275.0f, 0.0f);
        actor.Transform.Scale = 0.75f;

        _ = new SpriteComponent(actor)
        {
            Texture = _renderer.GetTexture("Assets/Radar.png")
        };
    }
    
    private void UnloadData()
    {
        // Delete actors
        // Because ~Actor calls RemoveActor, have to use a different style loop
        foreach (var actor in _actors.ToArray())
        {
            actor.Dispose();
        }
    }
}