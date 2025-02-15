using System.Diagnostics;
using Silk.NET.Maths;
using Veldrid;
using VeldridGame.Abstractions;
using VeldridGame.Assets;
using VeldridGame.Camera;
using VeldridGame.GameObjects;
using VeldridGame.Input;
using VeldridGame.Maths;
using VeldridGame.Rendering;
using VeldridGame.Resources;

namespace VeldridGame;

public class Game : IDisposable
{
    private const int FramesPerSecond = 60;
    private static readonly TimeSpan TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / FramesPerSecond);

    private readonly Screen _screen;
    private readonly Graphics _graphics;
    private readonly Renderer _renderer;
    private readonly InputSystem _inputSystem = new();

    private readonly SceneManager _sceneManager;
    private readonly AssetProvider _assetProvider;

    private CameraActor _cameraActor;

    public Game()
    {
        _screen = new Screen("Veldrid Game", new Vector2D<int>(1024, 768), new Vector2D<int>(100, 100));
        _graphics = new Graphics(_screen);
        _assetProvider = new(this, "Assets");
        _renderer = new Renderer(this, _screen, _graphics, _assetProvider);
        _sceneManager = new(this);
        
        _screen.Closed += (sender, args) => State = GameState.Quit;
    }
    
    public IScreen Screen => _screen;
    
    public Renderer Renderer => _renderer;
    
    public InputSystem InputSystem => _inputSystem;

    public ISceneManager SceneManager => _sceneManager;

    public IGraphics Graphics => _graphics;
    
    public IAssetProvider AssetProvider => _assetProvider;
    
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

    public void Dispose()
    {
        UnloadData();
        _graphics.Dispose();
        _renderer.Dispose();
        _assetProvider.Dispose();
    }
    
    private void ProcessInput()
    {
        var input = Screen.InternalWindow.PumpEvents();
        _inputSystem.Update(input);

        if (State == GameState.GamePlay)
        {
            HandleKeyPress( _inputSystem.State);
        }
    
        if (State == GameState.GamePlay)
        {
            _sceneManager.ProcessInput(_inputSystem.State);
        }
    }
    
    private void UpdateGame(float deltaTime)
    {
        if (State == GameState.Quit)
        {
            _screen.Close();
        }

        if (State == GameState.GamePlay)
        {
            _sceneManager.UpdateGame(deltaTime);
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
        var scene = new Scene(this);

        // Create actors
        var actor = new Actor(scene);
        actor.Transform.Position = new Vector3D<float>(200.0f, 75.0f, 0.0f);
        actor.Transform.Scale = 100.0f;

        var q = Quaternion<float>.CreateFromAxisAngle(Vector3D<float>.UnitY, -1 * Scalar<float>.PiOver2);
        q = Quaternion<float>.Concatenate(q, GameMath.CreateQuaternion(Vector3D<float>.UnitZ, (float)(Math.PI + Math.PI / 4.0f)));
        actor.Transform.Rotation = q;
        _ = new MeshComponent(actor)
        {
            Mesh = _renderer.GetMesh("Assets/Cube.gpmesh")
        };

        actor = new Actor(scene);
        actor.Transform.Position = new Vector3D<float>(200.0f, -75.0f, 0.0f);
        actor.Transform.Scale = 3.0f;
        
        _ = new MeshComponent(actor)
        {
            Mesh = _renderer.GetMesh("Assets/Sphere.gpmesh")
        };
        
        // Setup floor
        var start = -1250.0f;
        var size = 250.0f;
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                actor = new PlaneActor(scene);
                actor.Transform.Position = new Vector3D<float>(start + i * size, start + j * size, -100.0f);
            }
        }

        // Left/right walls
        q = GameMath.CreateQuaternion(Vector3D<float>.UnitX, Scalar<float>.PiOver2);
        for (int i = 0; i < 10; i++)
        {
            actor = new PlaneActor(scene);
            actor.Transform.Position = new Vector3D<float>(start + i * size, start - size, 0.0f);
            actor.Transform.Rotation = q;
        
            actor = new PlaneActor(scene);
            actor.Transform.Position = new Vector3D<float>(start + i * size, -start + size, 0.0f);
            actor.Transform.Rotation = q;
        }
        
        q = Quaternion<float>.Concatenate(q, GameMath.CreateQuaternion(Vector3D<float>.UnitZ, Scalar<float>.PiOver2));
        // Forward/back walls
        for (int i = 0; i < 10; i++)
        {
            actor = new PlaneActor(scene);
            actor.Transform.Position = new Vector3D<float>(start - size, start + i * size, 0.0f);
            actor.Transform.Rotation = q;
        
            actor = new PlaneActor(scene);
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
        _cameraActor = new CameraActor(scene);
        
        // UI elements
        actor = new Actor(scene);
        actor.Transform.Position = new Vector3D<float>(-350.0f, -350.0f, 0.0f);
            
        _ = new SpriteComponent(actor)
        {
            Texture = _assetProvider.LoadAsset<Texture2D>("HealthBar.png").Res,
        };

        actor = new Actor(scene);
        actor.Transform.Position = new Vector3D<float>(375.0f, -275.0f, 0.0f);
        actor.Transform.Scale = 0.75f;

        _ = new SpriteComponent(actor)
        {
            Texture = _assetProvider.LoadAsset<Texture2D>("Radar.png").Res,
        };
        
        _sceneManager.LoadScene(scene);
    }
    
    private void UnloadData()
    {
        _sceneManager.Clear();
    }
}