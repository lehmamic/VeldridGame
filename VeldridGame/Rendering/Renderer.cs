using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;

namespace VeldridGame.Rendering;

public class Renderer : IDisposable
{
    private const string VertexCode = @"
#version 450

layout(location = 0) in vec2 Position;
layout(location = 1) in vec4 Color;

layout(location = 0) out vec4 fsin_Color;

void main()
{
    gl_Position = vec4(Position, 0, 1);
    fsin_Color = Color;
}";

    private const string FragmentCode = @"
#version 450

layout(location = 0) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

void main()
{
    fsout_Color = fsin_Color;
}";
    
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Sdl2Window _window;
    private readonly DeviceBuffer _vertexBuffer;
    private readonly DeviceBuffer _indexBuffer;
    private readonly Shader[] _shaders;
    private readonly CommandList _commandList;
    private readonly Pipeline _pipeline;

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

        GraphicsDeviceOptions options = new GraphicsDeviceOptions
        {
            PreferStandardClipSpaceYDirection = true,
            PreferDepthRangeZeroToOne = true
        };

        _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_window, options, GraphicsBackend.OpenGL);
        
        ResourceFactory factory = _graphicsDevice.ResourceFactory;

        VertexPositionColor[] quadVertices =
        {
            new(new Vector2(-.75f, .75f), RgbaFloat.Red),
            new(new Vector2(.75f, .75f), RgbaFloat.Green),
            new(new Vector2(-.75f, -.75f), RgbaFloat.Blue),
            new(new Vector2(.75f, -.75f), RgbaFloat.Yellow)
        };
        BufferDescription vbDescription = new BufferDescription(
            4 * VertexPositionColor.SizeInBytes,
            BufferUsage.VertexBuffer);
        _vertexBuffer = factory.CreateBuffer(vbDescription);
        _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, quadVertices);

        ushort[] quadIndices = { 0, 1, 2, 3 };
        BufferDescription ibDescription = new BufferDescription(
            4 * sizeof(ushort),
            BufferUsage.IndexBuffer);
        _indexBuffer = factory.CreateBuffer(ibDescription);
        _graphicsDevice.UpdateBuffer(_indexBuffer, 0, quadIndices);

        VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

        ShaderDescription vertexShaderDesc = new ShaderDescription(
            ShaderStages.Vertex,
            Encoding.UTF8.GetBytes(VertexCode),
            "main");
        ShaderDescription fragmentShaderDesc = new ShaderDescription(
            ShaderStages.Fragment,
            Encoding.UTF8.GetBytes(FragmentCode),
            "main");

        _shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
        
        // Create pipeline
        GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
        pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
        pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
            depthTestEnabled: true,
            depthWriteEnabled: true,
            comparisonKind: ComparisonKind.LessEqual);
        pipelineDescription.RasterizerState = new RasterizerStateDescription(
            cullMode: FaceCullMode.Back,
            fillMode: PolygonFillMode.Solid,
            frontFace: FrontFace.Clockwise,
            depthClipEnabled: true,
            scissorTestEnabled: false);
        pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
        pipelineDescription.ResourceLayouts = Array.Empty<ResourceLayout>();
        pipelineDescription.ShaderSet = new ShaderSetDescription(
            vertexLayouts: [vertexLayout],
            shaders: _shaders);
        pipelineDescription.Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription;

        _pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

        _commandList = factory.CreateCommandList();
    }
    
    public Sdl2Window Window => _window;

    public void Draw()
    {
        // Begin() must be called before commands can be issued.
        _commandList.Begin();

        // We want to render directly to the output window.
        _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
        _commandList.ClearColorTarget(0, RgbaFloat.Black);

        // Set all relevant state to draw our quad.
        _commandList.SetVertexBuffer(0, _vertexBuffer);
        _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
        _commandList.SetPipeline(_pipeline);
        // Issue a Draw command for a single instance with 4 indices.
        _commandList.DrawIndexed(
            indexCount: 4,
            instanceCount: 1,
            indexStart: 0,
            vertexOffset: 0,
            instanceStart: 0);

        // End() must be called before commands can be submitted for execution.
        _commandList.End();
        _graphicsDevice.SubmitCommands(_commandList);

        // Once commands have been submitted, the rendered image can be presented to the application window.
        _graphicsDevice.SwapBuffers();
    }

    public void Dispose()
    {
        _commandList.Dispose();
        _indexBuffer.Dispose();
        _vertexBuffer.Dispose();

        foreach (var shader in _shaders)
        {
            shader.Dispose();
        }
        
        _pipeline.Dispose();
        _graphicsDevice.Dispose();
    }
}