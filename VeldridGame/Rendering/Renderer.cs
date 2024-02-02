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
    
    private readonly List<MeshComponent> _meshComps = new();
    private readonly Dictionary<string, Mesh> _meshes = new();
    
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Sdl2Window _window;
    
    private readonly Shader _meshShader;

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

        // Make sure we can load and compile shaders
        _meshShader = new Shader(_graphicsDevice, "Shaders/Pong.vert", "Shaders/Pong.frag");

        // Set the view-projection matrix
        ViewMatrix = GameMath.CreateLookAt(Vector3D<float>.Zero, Vector3D<float>.UnitX, Vector3D<float>.UnitZ);
        ProjectionMatrix = GameMath.CreatePerspectiveFieldOfView(
            Scalar.DegreesToRadians(70.0f),
            (float) Window.Width, Window.Height,
            25.0f,                  // Near plane
            10000.0f);

        var factory = _graphicsDevice.ResourceFactory;
        _commandList = factory.CreateCommandList();
    }

    public Sdl2Window Window => _window;
    
    public GraphicsDevice GraphicsDevice => _graphicsDevice;
    
    public Matrix4X4<float> ViewMatrix { get; set; }

    public Matrix4X4<float> ProjectionMatrix { get; set; }

    public void Draw()
    {
        _commandList.Begin();

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
    
    private static VertexPositionTexture[] GetCubeVertices()
    {
        VertexPositionTexture[] vertices =
        [
            // Top
            new VertexPositionTexture(new Vector3D<float>(-0.5f, +0.5f, -0.5f), new Vector2D<float>(0, 0)),
            new VertexPositionTexture(new Vector3D<float>(+0.5f, +0.5f, -0.5f), new Vector2D<float>(1, 0)),
            new VertexPositionTexture(new Vector3D<float>(+0.5f, +0.5f, +0.5f), new Vector2D<float>(1, 1)),
            new VertexPositionTexture(new Vector3D<float>(-0.5f, +0.5f, +0.5f), new Vector2D<float>(0, 1)),
            // Bottom                                                             
            new VertexPositionTexture(new Vector3D<float>(-0.5f,-0.5f, +0.5f),  new Vector2D<float>(0, 0)),
            new VertexPositionTexture(new Vector3D<float>(+0.5f,-0.5f, +0.5f),  new Vector2D<float>(1, 0)),
            new VertexPositionTexture(new Vector3D<float>(+0.5f,-0.5f, -0.5f),  new Vector2D<float>(1, 1)),
            new VertexPositionTexture(new Vector3D<float>(-0.5f,-0.5f, -0.5f),  new Vector2D<float>(0, 1)),
            // Left                                                               
            new VertexPositionTexture(new Vector3D<float>(-0.5f, +0.5f, -0.5f), new Vector2D<float>(0, 0)),
            new VertexPositionTexture(new Vector3D<float>(-0.5f, +0.5f, +0.5f), new Vector2D<float>(1, 0)),
            new VertexPositionTexture(new Vector3D<float>(-0.5f, -0.5f, +0.5f), new Vector2D<float>(1, 1)),
            new VertexPositionTexture(new Vector3D<float>(-0.5f, -0.5f, -0.5f), new Vector2D<float>(0, 1)),
            // Right                                                              
            new VertexPositionTexture(new Vector3D<float>(+0.5f, +0.5f, +0.5f), new Vector2D<float>(0, 0)),
            new VertexPositionTexture(new Vector3D<float>(+0.5f, +0.5f, -0.5f), new Vector2D<float>(1, 0)),
            new VertexPositionTexture(new Vector3D<float>(+0.5f, -0.5f, -0.5f), new Vector2D<float>(1, 1)),
            new VertexPositionTexture(new Vector3D<float>(+0.5f, -0.5f, +0.5f), new Vector2D<float>(0, 1)),
            // Back                                                               
            new VertexPositionTexture(new Vector3D<float>(+0.5f, +0.5f, -0.5f), new Vector2D<float>(0, 0)),
            new VertexPositionTexture(new Vector3D<float>(-0.5f, +0.5f, -0.5f), new Vector2D<float>(1, 0)),
            new VertexPositionTexture(new Vector3D<float>(-0.5f, -0.5f, -0.5f), new Vector2D<float>(1, 1)),
            new VertexPositionTexture(new Vector3D<float>(+0.5f, -0.5f, -0.5f), new Vector2D<float>(0, 1)),
            // Front                                                              
            new VertexPositionTexture(new Vector3D<float>(-0.5f, +0.5f, +0.5f), new Vector2D<float>(0, 0)),
            new VertexPositionTexture(new Vector3D<float>(+0.5f, +0.5f, +0.5f), new Vector2D<float>(1, 0)),
            new VertexPositionTexture(new Vector3D<float>(+0.5f, -0.5f, +0.5f), new Vector2D<float>(1, 1)),
            new VertexPositionTexture(new Vector3D<float>(-0.5f, -0.5f, +0.5f), new Vector2D<float>(0, 1))
        ];

        return vertices;
    }

    private static ushort[] GetCubeIndices()
    {
        ushort[] indices =
        [
            0,1,2, 0,2,3,
            4,5,6, 4,6,7,
            8,9,10, 8,10,11,
            12,13,14, 12,14,15,
            16,17,18, 16,18,19,
            20,21,22, 20,22,23
        ];

        return indices;
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