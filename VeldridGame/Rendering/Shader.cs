using Veldrid;
using Veldrid.SPIRV;

namespace VeldridGame.Rendering;

public class Shader : IDisposable
{
    private readonly Pipeline _pipeline;
    private readonly ResourceLayout _projViewLayout;
    private readonly ResourceLayout _worldTransformLayout;
    private readonly ResourceLayout _lightInfoLayout;
    private readonly ResourceLayout _materialLayout;
    private readonly ResourceLayout _textureLayout;
    private readonly DeviceBuffer _projectionBuffer;
    private readonly DeviceBuffer _viewBuffer;
    private readonly DeviceBuffer _worldBuffer;
    private readonly DeviceBuffer _cameraPositionBuffer;
    private readonly DeviceBuffer _ambientLightBuffer;
    private readonly DeviceBuffer _directionalLightBuffer;
    private readonly DeviceBuffer _materialBuffer;
    private readonly ResourceSet _projViewSet;
    private readonly ResourceSet _worldTransformSet;
    private readonly ResourceSet _lightInfoSet;
    private readonly ResourceSet _materialSet;

    public Shader(GraphicsDevice graphicsDevice, string vertexShaderFilePath, string fragmentShaderFilePath)
    {
        var factory = graphicsDevice.ResourceFactory;
        
        ShaderSetDescription shaderSet = new ShaderSetDescription(
            new[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
            },
            factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, File.ReadAllBytes(vertexShaderFilePath), "main"),
                new ShaderDescription(ShaderStages.Fragment, File.ReadAllBytes(fragmentShaderFilePath), "main")),
            ShaderHelper.GetSpecializations(graphicsDevice));
        
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
        
        _cameraPositionBuffer = factory.CreateBuffer(new BufferDescription(16, BufferUsage.UniformBuffer));
        _ambientLightBuffer = factory.CreateBuffer(new BufferDescription(16, BufferUsage.UniformBuffer));
        _directionalLightBuffer = factory.CreateBuffer(new BufferDescription(48, BufferUsage.UniformBuffer));
        _lightInfoLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("CameraPosition", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("AmbientLight", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("DirectionalLight", ResourceKind.UniformBuffer, ShaderStages.Fragment)));
        _lightInfoSet = factory.CreateResourceSet(new ResourceSetDescription(
            _lightInfoLayout,
            _cameraPositionBuffer,
            _ambientLightBuffer,
            _directionalLightBuffer));
        
        _materialBuffer = factory.CreateBuffer(new BufferDescription(16, BufferUsage.UniformBuffer));
        _materialLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SpecularPower", ResourceKind.UniformBuffer, ShaderStages.Fragment)));
        _materialSet = factory.CreateResourceSet(new ResourceSetDescription(
            _materialLayout,
            _materialBuffer));

        _textureLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

        _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
            BlendStateDescription.SingleOverrideBlend,
            DepthStencilStateDescription.DepthOnlyLessEqual,
            RasterizerStateDescription.CullNone with { FrontFace = FrontFace.CounterClockwise },
            PrimitiveTopology.TriangleList,
            shaderSet,
            [_projViewLayout, _worldTransformLayout, _lightInfoLayout, _materialLayout, _textureLayout],
            graphicsDevice.MainSwapchain.Framebuffer.OutputDescription));
    }
    
    public DeviceBuffer ProjectionBuffer => _projectionBuffer;
    
    public DeviceBuffer ViewBuffer => _viewBuffer;
    
    public DeviceBuffer WorldBuffer => _worldBuffer;
    
    public DeviceBuffer CameraPositionBuffer => _cameraPositionBuffer;
    
    public DeviceBuffer AmbientLightBuffer => _ambientLightBuffer;
    
    public DeviceBuffer DirectionalLightBuffer => _directionalLightBuffer;
    
    public DeviceBuffer MaterialBuffer => _materialBuffer;
    
    public ResourceLayout TextureLayout => _textureLayout;

    public void SetActive(CommandList commandList)
    {
        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, _projViewSet);
        commandList.SetGraphicsResourceSet(1, _worldTransformSet);
        commandList.SetGraphicsResourceSet(2, _lightInfoSet);
        commandList.SetGraphicsResourceSet(3, _materialSet);
    }

    public void Dispose()
    {
        _projViewLayout.Dispose();
        _worldTransformLayout.Dispose();
        _textureLayout.Dispose();
        _materialLayout.Dispose();
        _lightInfoLayout.Dispose();
        _projectionBuffer.Dispose();
        _viewBuffer.Dispose();
        _worldBuffer.Dispose();
        _cameraPositionBuffer.Dispose();
        _ambientLightBuffer.Dispose();
        _directionalLightBuffer.Dispose();
        _materialBuffer.Dispose();
        _projViewSet.Dispose();
        _worldTransformSet.Dispose();
        _lightInfoSet.Dispose();
        _materialSet.Dispose();
        _pipeline.Dispose();
    }
}