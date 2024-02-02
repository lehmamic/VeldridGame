using Veldrid;
using Veldrid.SPIRV;
using VeldridGame.Camera;

namespace VeldridGame.Rendering;

public class Shader : IDisposable
{
    private readonly Pipeline _pipeline;
    private readonly ResourceLayout _projViewLayout;
    private readonly ResourceLayout _worldTransformLayout;
    private readonly ResourceLayout _lightInfoLayout;
    private readonly ResourceLayout _materialLayout;
    private readonly ResourceLayout _textureLayout;
    private readonly ResourceSet _projViewSet;
    private readonly ResourceSet _worldTransformSet;
    private readonly ResourceSet _lightInfoSet;
    private readonly ResourceSet _materialSet;
    private readonly Dictionary<string, DeviceBuffer> _bufferMap = new();

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
        
        _bufferMap[ShaderUniforms.ProjectionBuffer] = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
        _bufferMap[ShaderUniforms.ViewBuffer] = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
        _projViewLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(ShaderUniforms.ProjectionBuffer, ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription(ShaderUniforms.ViewBuffer, ResourceKind.UniformBuffer, ShaderStages.Vertex)));
        _projViewSet = factory.CreateResourceSet(new ResourceSetDescription(
            _projViewLayout,
            _bufferMap[ShaderUniforms.ProjectionBuffer],
            _bufferMap[ShaderUniforms.ViewBuffer]));
        
        _bufferMap[ShaderUniforms.WorldBuffer] = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
        _worldTransformLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(ShaderUniforms.WorldBuffer, ResourceKind.UniformBuffer, ShaderStages.Vertex)));
        _worldTransformSet = factory.CreateResourceSet(new ResourceSetDescription(
            _worldTransformLayout,
            _bufferMap[ShaderUniforms.WorldBuffer]));
        
        _bufferMap[ShaderUniforms.CameraBuffer] = factory.CreateBuffer(new BufferDescription(CameraInfo.SizeInBytes, BufferUsage.UniformBuffer));
        _bufferMap[ShaderUniforms.AmbientLightBuffer] = factory.CreateBuffer(new BufferDescription(AmbientLightInfo.SizeInBytes, BufferUsage.UniformBuffer));
        _bufferMap[ShaderUniforms.DirectionalLightBuffer] = factory.CreateBuffer(new BufferDescription(DirectionalLightInfo.SizeInBytes, BufferUsage.UniformBuffer));
        _lightInfoLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(ShaderUniforms.CameraBuffer, ResourceKind.UniformBuffer, ShaderStages.Fragment),
                new ResourceLayoutElementDescription(ShaderUniforms.AmbientLightBuffer, ResourceKind.UniformBuffer, ShaderStages.Fragment),
                new ResourceLayoutElementDescription(ShaderUniforms.DirectionalLightBuffer, ResourceKind.UniformBuffer, ShaderStages.Fragment)));
        _lightInfoSet = factory.CreateResourceSet(new ResourceSetDescription(
            _lightInfoLayout,
            _bufferMap[ShaderUniforms.CameraBuffer],
            _bufferMap[ShaderUniforms.AmbientLightBuffer],
            _bufferMap[ShaderUniforms.DirectionalLightBuffer]));
        
        _bufferMap[ShaderUniforms.MaterialBuffer] = factory.CreateBuffer(new BufferDescription(16, BufferUsage.UniformBuffer));
        _materialLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(ShaderUniforms.MaterialBuffer, ResourceKind.UniformBuffer, ShaderStages.Fragment)));
        _materialSet = factory.CreateResourceSet(new ResourceSetDescription(
            _materialLayout,
            _bufferMap[ShaderUniforms.MaterialBuffer]));

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
    
    public ResourceLayout TextureLayout => _textureLayout;

    public void SetActive(CommandList commandList)
    {
        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, _projViewSet);
        commandList.SetGraphicsResourceSet(1, _worldTransformSet);
        commandList.SetGraphicsResourceSet(2, _lightInfoSet);
        commandList.SetGraphicsResourceSet(3, _materialSet);
    }
    
    public void SetUniform<T>(CommandList commandList, string buffer, T data)
        where T : unmanaged
    {
        commandList.UpdateBuffer(_bufferMap[buffer], 0, data);
    }

    public void Dispose()
    {
        foreach (var buffer in _bufferMap.Values)
        {
            buffer.Dispose();
        }
        _projViewLayout.Dispose();
        _worldTransformLayout.Dispose();
        _textureLayout.Dispose();
        _materialLayout.Dispose();
        _lightInfoLayout.Dispose();
        _projViewSet.Dispose();
        _worldTransformSet.Dispose();
        _lightInfoSet.Dispose();
        _materialSet.Dispose();
        _pipeline.Dispose();
    }
}