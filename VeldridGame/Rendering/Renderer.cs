using System.Numerics;
using System.Text;
using Silk.NET.Maths;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;
using VeldridGame.Maths.Geometry;

namespace VeldridGame.Rendering;

public class Renderer : IDisposable
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Sdl2Window _window;
    
    private readonly Shader _meshShader;

    private readonly CommandList _commandList;

    private readonly Mesh _mesh;

    // Map of textures loaded
    private readonly Dictionary<string, Texture> _textures = new();

    private float _ticks;


    public Renderer(int width, int height, string title)
    {
        var windowCi = new WindowCreateInfo
        {
            X = 100,
            Y = 100,
            WindowWidth = width,
            WindowHeight = height,
            WindowTitle = title
        };
        _window = VeldridStartup.CreateWindow(ref windowCi);
        
        var options = new GraphicsDeviceOptions(
            debug: false,
            swapchainDepthFormat: PixelFormat.R16_UNorm,
            syncToVerticalBlank: true,
            resourceBindingModel: ResourceBindingModel.Improved,
            preferDepthRangeZeroToOne: true,
            preferStandardClipSpaceYDirection: true);

        _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_window, options, GraphicsBackend.OpenGL);

        // Make sure we can load and compile shaders
        _meshShader = new Shader(_graphicsDevice, "Shaders/SampleCube.vert", "Shaders/SampleCube.frag");

        _mesh = new Mesh(
            radius: 0,
            specularPower: 0,
            shaderName: "simple",
            textures: new List<Texture> {GetTexture("Assets/Textures/spnza_bricks_a_diff.png")},
            box: new AABB(Vector3D<float>.Zero, Vector3D<float>.Zero),
            vertexArrayObject: new VertexArrayObject(_graphicsDevice, GetCubeVertices(), GetCubeIndices()));

        var factory = _graphicsDevice.ResourceFactory;
        _commandList = factory.CreateCommandList();
    }
    
    public Sdl2Window Window => _window;
    
    public GraphicsDevice GraphicsDevice => _graphicsDevice;

    public void Draw(float deltaTime)
    {
        _ticks += deltaTime * 1000f;
        _commandList.Begin();

        _commandList.UpdateBuffer(_meshShader.ProjectionBuffer, 0, Matrix4X4.CreatePerspectiveFieldOfView(
            1.0f,
            (float)Window.Width / Window.Height,
            0.5f,
            100f));

        _commandList.UpdateBuffer(_meshShader.ViewBuffer, 0, Matrix4X4.CreateLookAt(Vector3D<float>.UnitZ * 2.5f, Vector3D<float>.Zero, Vector3D<float>.UnitY));

        Matrix4x4 rotation =
            Matrix4x4.CreateFromAxisAngle(Vector3.UnitY, (_ticks / 1000f))
            * Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, (_ticks / 3000f));
        _commandList.UpdateBuffer(_meshShader.WorldBuffer, 0, ref rotation);

        _commandList.SetFramebuffer(_graphicsDevice.MainSwapchain.Framebuffer);
        _commandList.ClearColorTarget(0, RgbaFloat.Black);
        _commandList.ClearDepthStencil(1f);

        _meshShader.SetActive(_commandList);
        _mesh.VertexArrayObject.SetActive(_commandList);
        _mesh.GetTexture(0)?.SetActive(_commandList, 2);
        _commandList.DrawIndexed(36, 1, 0, 0, 0);

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
        
        _mesh.Dispose();

        _meshShader.Dispose();
        _graphicsDevice.Dispose();
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
}