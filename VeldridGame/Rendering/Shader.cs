using Veldrid;
using Veldrid.SPIRV;

namespace VeldridGame.Rendering;

public class Shader : IDisposable
{
    private readonly Pipeline _pipeline;
    private readonly ResourceLayout _projViewLayout;
    private readonly ResourceLayout _worldTransformLayout;
    private readonly ResourceLayout _textureLayout;
    private readonly DeviceBuffer _projectionBuffer;
    private readonly DeviceBuffer _viewBuffer;
    private readonly DeviceBuffer _worldBuffer;
    private readonly ResourceSet _projViewSet;
    private readonly ResourceSet _worldTransformSet;

    public Shader(GraphicsDevice graphicsDevice, string vertexShaderFilePath, string fragmentShaderFilePath)
    {
        var factory = graphicsDevice.ResourceFactory;
        
        ShaderSetDescription shaderSet = new ShaderSetDescription(
            new[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
            },
            factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, File.ReadAllBytes(vertexShaderFilePath), "main"),
                new ShaderDescription(ShaderStages.Fragment, File.ReadAllBytes(fragmentShaderFilePath), "main")));
        
        _projectionBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
        _viewBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
        _projViewLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("ViewBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)));
        _projViewSet = factory.CreateResourceSet(new ResourceSetDescription(
            _projViewLayout,
            _projectionBuffer,
            _viewBuffer));
        
        _worldBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
        _worldTransformLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("WorldBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)));
        _worldTransformSet = factory.CreateResourceSet(new ResourceSetDescription(
            _worldTransformLayout,
            _worldBuffer));

        _textureLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

        _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
            BlendStateDescription.SingleOverrideBlend,
            DepthStencilStateDescription.DepthOnlyLessEqual,
            RasterizerStateDescription.Default,
            PrimitiveTopology.TriangleList,
            shaderSet,
            [_projViewLayout, _worldTransformLayout, _textureLayout],
            graphicsDevice.MainSwapchain.Framebuffer.OutputDescription));
    }
    
    public DeviceBuffer ProjectionBuffer => _projectionBuffer;
    
    public DeviceBuffer ViewBuffer => _viewBuffer;
    
    public DeviceBuffer WorldBuffer => _worldBuffer;
    
    public ResourceLayout TextureLayout => _textureLayout;

    public void SetActive(CommandList commandList)
    {
        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, _projViewSet);
        commandList.SetGraphicsResourceSet(1, _worldTransformSet);
    }

    public void Dispose()
    {
        _projViewLayout.Dispose();
        _worldTransformLayout.Dispose();
        _textureLayout.Dispose();
        _projectionBuffer.Dispose();
        _viewBuffer.Dispose();
        _worldBuffer.Dispose();
        _projViewSet.Dispose();
        _worldTransformSet.Dispose();
        _pipeline.Dispose();
    }
}