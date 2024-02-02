using Silk.NET.Maths;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using VeldridGame.Abstractions;
using VeldridGame.Camera;
using VeldridGame.Maths;

namespace VeldridGame.Rendering;

public class Renderer : IDisposable
{
    private readonly Game _game;

    private readonly List<SpriteComponent> _sprites = new();
    private readonly List<MeshComponent> _meshComps = new();
    private readonly Dictionary<string, Mesh> _meshes = new();
    
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Sdl2Window _window;

    private readonly Shader _spriteShader;
    private readonly Shader _meshShader;
    
    private VertexArrayObject _spriteVertices;

    private readonly CommandList _commandList;

    // Map of textures loaded
    private readonly Dictionary<string, Texture> _textures = new();


    // Lighting data
    public AmbientLightInfo AmbientLight { get; set; }
    public DirectionalLightInfo DirectionalLightInfo { get; set; }

    public Renderer(Game game, int width, int height, string title)
    {
        _game = game;
        var windowCi = new WindowCreateInfo
        {
            X = 100,
            Y = 100,
            WindowWidth = width,
            WindowHeight = height,
            WindowTitle = title
        };
        _window = VeldridStartup.CreateWindow(ref windowCi);
        _window.Closed += OnWindowClosed;
        
        var options = new GraphicsDeviceOptions(
            debug: true,
            swapchainDepthFormat: PixelFormat.R16_UNorm,
            syncToVerticalBlank: true,
            resourceBindingModel: ResourceBindingModel.Improved,
            preferDepthRangeZeroToOne: true,
            preferStandardClipSpaceYDirection: true);

        _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_window, options, GraphicsBackend.OpenGL);
        
        var factory = _graphicsDevice.ResourceFactory;
        _commandList = factory.CreateCommandList();
        _commandList.Begin();

        // Make sure we can load and compile shaders
        _spriteShader = new SpriteShader(_graphicsDevice, "Shaders/Sprite.vert", "Shaders/Sprite.frag");
        _spriteShader.SetActive(_commandList);
        
        // Set the view-projection matrix
        var spriteViewProj = GameMath.CreateSimpleViewProj(Window.Width, Window.Height);
        _spriteShader.SetUniform(_commandList, ShaderUniforms.ViewBuffer, spriteViewProj);
        
        _meshShader = new MeshShader(_graphicsDevice, "Shaders/Pong.vert", "Shaders/Pong.frag");
        _meshShader.SetActive(_commandList);

        // Set the view-projection matrix
        ViewMatrix = GameMath.CreateLookAt(Vector3D<float>.Zero, Vector3D<float>.UnitX, Vector3D<float>.UnitZ);
        _meshShader.SetUniform(_commandList, ShaderUniforms.ViewBuffer, ViewMatrix);

        ProjectionMatrix = GameMath.CreatePerspectiveFieldOfView(
            Scalar.DegreesToRadians(70.0f),
            (float) Window.Width, Window.Height,
            25.0f,                  // Near plane
            10000.0f);
        _meshShader.SetUniform(_commandList, ShaderUniforms.ProjectionBuffer, ProjectionMatrix);
        
        // Create quad for drawing sprites
        CreateSpriteVertices();
        
        _commandList.End();
        _graphicsDevice.SubmitCommands(_commandList);
        _graphicsDevice.WaitForIdle();
    }

    public Sdl2Window Window => _window;
    
    public GraphicsDevice GraphicsDevice => _graphicsDevice;
    
    public Matrix4X4<float> ViewMatrix { get; set; }

    public Matrix4X4<float> ProjectionMatrix { get; set; }

    public void Draw()
    {
        _commandList.Begin();
        
        /*
         * Draw 3d scene to the frame buffers
         */

        // Set the current frame buffer
        _commandList.SetFramebuffer(_graphicsDevice.MainSwapchain.Framebuffer);
        
        // Clear color buffer/depth buffer
        _commandList.ClearColorTarget(0, RgbaFloat.Black);
        _commandList.ClearDepthStencil(1f);

        // Set the basic mesh shader active
        _meshShader.SetActive(_commandList);

        // Update view-projection matrix
        _meshShader.SetUniform(_commandList, ShaderUniforms.ViewBuffer, ViewMatrix);
        _meshShader.SetUniform(_commandList, ShaderUniforms.ProjectionBuffer, ProjectionMatrix);
        
        // Update lighting uniforms
        SetLightUniforms(_meshShader);

        // Draw all meshes
        foreach (var mesh in _meshComps)
        {
            if (mesh.Visible)
            {
                mesh.Draw(_commandList, _meshShader);
            }
        }
        
        /*
         * Draw all sprite components
         */

        // Set sprite shader and vertex array objects active
        _spriteShader.SetActive(_commandList);
        _spriteVertices.SetActive(_commandList);

        // Draw all sprites
        foreach (var sprite in _sprites)
        {
            if (sprite.Visible)
            {
                sprite.Draw(_commandList, _spriteShader);
            }
        }
        
        _commandList.End();

        _graphicsDevice.SubmitCommands(_commandList);
        _graphicsDevice.SwapBuffers(_graphicsDevice.MainSwapchain);
        _graphicsDevice.WaitForIdle();
    }
    
    public Texture GetTexture(string fileName)
    {
        if (!_textures.ContainsKey(fileName))
        {
            var texture = new Texture(_graphicsDevice , fileName);
            _textures.Add(fileName, texture);
        }

        return _textures[fileName];
    }

    public void AddSprite(SpriteComponent sprite)
    {
        // Find the insertion point in the sorted vector
        // (The first element with a order higher than me)
        int index = 0;
        for (; index < _sprites.Count; index++)
        {
            if (sprite.DrawOrder < _sprites[index].DrawOrder)
            {
                break;
            }
        }

        // Inserts element before position of iterator
        _sprites.Insert(index, sprite);
    }

    public void RemoveSprite(SpriteComponent sprite)
    {
        _sprites.Remove(sprite);
    }

    public void AddMeshComp(MeshComponent mesh)
    {
        if (mesh.IsSkeletal)
        {
            // _skeletalMeshes.Add((SkeletalMeshComponent)mesh);
        }
        else
        {
            _meshComps.Add(mesh);
        }
    }

    public void RemoveMeshComp(MeshComponent mesh)
    {
        if (mesh.IsSkeletal)
        {
            // _skeletalMeshes.Remove((SkeletalMeshComponent) mesh);
        }
        else
        {
            _meshComps.Remove(mesh);
        }
    }

    public Mesh GetMesh(string fileName)
    {
        if (!_meshes.ContainsKey(fileName))
        {
            var mesh = Mesh.Load(fileName, _game);
            _meshes.Add(fileName, mesh);
        }

        return _meshes[fileName];
    }

    public void Dispose()
    {
        _commandList.Dispose();
        
        // Destroy textures
        foreach (var texture in _textures.ToArray())
        {
            _textures.Remove(texture.Key);
            texture.Value.Dispose();
        }

        // Destroy meshes
        foreach (var mesh in _meshes.ToArray())
        {
            _meshes.Remove(mesh.Key);
            mesh.Value.Dispose();
        }

        _meshShader.Dispose();
        _graphicsDevice.Dispose();
    }
    
    private void CreateSpriteVertices()
    {
        var vertices = new[] {
            // vertex(3)/normal(3)/(uv coord)
            new VertexPositionNormalTexture(new(-0.5f, 0.5f, 0.0f), new(0.0f, 0.0f, 0.0f), new(0.0f, 0.0f)), // top left
            new VertexPositionNormalTexture(new(0.5f, 0.5f, 0.0f), new(0.0f, 0.0f, 0.0f), new(1.0f, 0.0f)), // top right
            new VertexPositionNormalTexture(new(0.5f, -0.5f, 0.0f), new(0.0f, 0.0f, 0.0f), new(1.0f, 1.0f)), // bottom right
            new VertexPositionNormalTexture(new(-0.5f, -0.5f, 0.0f), new(0.0f, 0.0f, 0.0f), new(0.0f, 1.0f)),  // bottom left
        };

        var indices = new ushort[] {
            0, 1, 2,
            2, 3, 0
        };

        _spriteVertices = new VertexArrayObject(_graphicsDevice, vertices, indices);
    }

    private void SetLightUniforms(Shader shader)
    {
        // Camera position is from inverted view
        Matrix4X4.Invert(ViewMatrix, out var invView);
        shader.SetUniform(_commandList, ShaderUniforms.CameraBuffer, new CameraInfo(invView.GetTranslation()));

        // Ambient light
        shader.SetUniform(_commandList, ShaderUniforms.AmbientLightBuffer, AmbientLight);
    
        // Directional light
        shader.SetUniform(_commandList, ShaderUniforms.DirectionalLightBuffer, DirectionalLightInfo);
    }
    
    private void OnWindowClosed()
    {
        _game.State = GameState.Quit;
    }
}