using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;

namespace VeldridGame.Rendering;

public class Renderer : IDisposable
{
    private const string VertexCode = @"
#version 450

layout(set = 0, binding = 0) uniform ProjectionBuffer
{
    mat4 Projection;
};

layout(set = 0, binding = 1) uniform ViewBuffer
{
    mat4 View;
};

layout(set = 0, binding = 2) uniform WorldBuffer
{
    mat4 World;
};

layout(location = 0) in vec3 Position;
layout(location = 1) in vec2 TexCoords;

layout(location = 0) out vec2 fsin_texCoords;

void main()
{
    vec4 worldPosition = World * vec4(Position, 1);
    vec4 viewPosition = View * worldPosition;
    vec4 clipPosition = Projection * viewPosition;
    gl_Position = clipPosition;
    fsin_texCoords = TexCoords;
}";

    private const string FragmentCode = @"
#version 450

layout(location = 0) in vec2 fsin_texCoords;
layout(location = 0) out vec4 fsout_color;

layout(set = 1, binding = 0) uniform texture2D SurfaceTexture;
layout(set = 1, binding = 1) uniform sampler SurfaceSampler;

void main()
{
    fsout_color =  texture(sampler2D(SurfaceTexture, SurfaceSampler), fsin_texCoords);
}";
    
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Sdl2Window _window;
    private readonly VertexArrayObject _vao;
    private readonly Shader[] _shaders;
    private readonly CommandList _commandList;
    private readonly Pipeline _pipeline;
    private readonly DeviceBuffer _projectionBuffer;
    private readonly DeviceBuffer _viewBuffer;
    private readonly DeviceBuffer _worldBuffer;

    private readonly ResourceSet _projViewWorldSet;
    private readonly Texture _texture;

    private readonly VertexPositionTexture[] _vertices;
    private readonly ushort[] _indices;
    
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
        
        ResourceFactory factory = _graphicsDevice.ResourceFactory;
        
        _vertices = GetCubeVertices();
        _indices = GetCubeIndices();

        _projectionBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
        _viewBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
        _worldBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));

        _vao = new VertexArrayObject(_graphicsDevice, _vertices, _indices);

        ShaderSetDescription shaderSet = new ShaderSetDescription(
            new[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
            },
            factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(VertexCode), "main"),
                new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(FragmentCode), "main")));

        ResourceLayout projViewWorldLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("ViewBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("WorldBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

        ResourceLayout textureLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

        _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
            BlendStateDescription.SingleOverrideBlend,
            DepthStencilStateDescription.DepthOnlyLessEqual,
            RasterizerStateDescription.Default,
            PrimitiveTopology.TriangleList,
            shaderSet,
            [projViewWorldLayout, textureLayout],
            _graphicsDevice.MainSwapchain.Framebuffer.OutputDescription));

        _projViewWorldSet = factory.CreateResourceSet(new ResourceSetDescription(
            projViewWorldLayout,
            _projectionBuffer,
            _viewBuffer,
            _worldBuffer));
        
        _texture = new Texture(_graphicsDevice, textureLayout, "Assets/Textures/spnza_bricks_a_diff.png");

        _commandList = factory.CreateCommandList();
    }
    
    public Sdl2Window Window => _window;

    public void Draw(float deltaTime)
    {
        _ticks += deltaTime * 1000f;
        _commandList.Begin();

        _commandList.UpdateBuffer(_projectionBuffer, 0, Matrix4x4.CreatePerspectiveFieldOfView(
            1.0f,
            (float)Window.Width / Window.Height,
            0.5f,
            100f));

        _commandList.UpdateBuffer(_viewBuffer, 0, Matrix4x4.CreateLookAt(Vector3.UnitZ * 2.5f, Vector3.Zero, Vector3.UnitY));

        Matrix4x4 rotation =
            Matrix4x4.CreateFromAxisAngle(Vector3.UnitY, (_ticks / 1000f))
            * Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, (_ticks / 3000f));
        _commandList.UpdateBuffer(_worldBuffer, 0, ref rotation);

        _commandList.SetFramebuffer(_graphicsDevice.MainSwapchain.Framebuffer);
        _commandList.ClearColorTarget(0, RgbaFloat.Black);
        _commandList.ClearDepthStencil(1f);
        _commandList.SetPipeline(_pipeline);
        _vao.SetActive(_commandList);
        _commandList.SetGraphicsResourceSet(0, _projViewWorldSet);
        _texture.SetActive(_commandList, 1);
        _commandList.DrawIndexed(36, 1, 0, 0, 0);

        _commandList.End();

        _graphicsDevice.SubmitCommands(_commandList);
        _graphicsDevice.SwapBuffers(_graphicsDevice.MainSwapchain);
        _graphicsDevice.WaitForIdle();
    }

    public void Dispose()
    {
        _commandList.Dispose();
        
        _vao.Dispose();
        _projectionBuffer.Dispose();
        _viewBuffer.Dispose();
        _worldBuffer.Dispose();
        
        _projViewWorldSet.Dispose();

        foreach (var shader in _shaders)
        {
            shader.Dispose();
        }
        
        _texture.Dispose();
        
        _pipeline.Dispose();
        _graphicsDevice.Dispose();
    }
    
    private static VertexPositionTexture[] GetCubeVertices()
    {
        VertexPositionTexture[] vertices =
        [
            // Top
            new VertexPositionTexture(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(0, 1)),
            // Bottom                                                             
            new VertexPositionTexture(new Vector3(-0.5f,-0.5f, +0.5f),  new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(+0.5f,-0.5f, +0.5f),  new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(+0.5f,-0.5f, -0.5f),  new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-0.5f,-0.5f, -0.5f),  new Vector2(0, 1)),
            // Left                                                               
            new VertexPositionTexture(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(0, 1)),
            // Right                                                              
            new VertexPositionTexture(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(+0.5f, -0.5f, -0.5f), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(+0.5f, -0.5f, +0.5f), new Vector2(0, 1)),
            // Back                                                               
            new VertexPositionTexture(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(+0.5f, -0.5f, -0.5f), new Vector2(0, 1)),
            // Front                                                              
            new VertexPositionTexture(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(+0.5f, -0.5f, +0.5f), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(0, 1))
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