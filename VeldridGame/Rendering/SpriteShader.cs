using Veldrid;
using Veldrid.SPIRV;

namespace VeldridGame.Rendering;

public class SpriteShader : Shader
{
    private readonly ResourceLayout _viewLayout;
    private readonly ResourceLayout _worldTransformLayout;
    private readonly ResourceLayout _textureLayout;
    private readonly ResourceSet _viewSet;
    private readonly ResourceSet _worldTransformSet;
    private readonly Pipeline _pipeline;

    public SpriteShader(GraphicsDevice graphicsDevice, string vertexShaderFilePath, string fragmentShaderFilePath)
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

        BufferMap[ShaderUniforms.ViewBuffer] = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
        _viewLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(ShaderUniforms.ViewBuffer, ResourceKind.UniformBuffer, ShaderStages.Vertex)));
        _viewSet = factory.CreateResourceSet(new ResourceSetDescription(
            _viewLayout,
            BufferMap[ShaderUniforms.ViewBuffer]));
        
        BufferMap[ShaderUniforms.WorldBuffer] = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
        _worldTransformLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(ShaderUniforms.WorldBuffer, ResourceKind.UniformBuffer, ShaderStages.Vertex)));
        _worldTransformSet = factory.CreateResourceSet(new ResourceSetDescription(
            _worldTransformLayout,
            BufferMap[ShaderUniforms.WorldBuffer]));

        _textureLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

        _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
            BlendStateDescription.SingleAlphaBlend,
            DepthStencilStateDescription.Disabled,
            RasterizerStateDescription.CullNone with { FrontFace = FrontFace.CounterClockwise },
            PrimitiveTopology.TriangleList,
            shaderSet,
            [_viewLayout, _worldTransformLayout, _textureLayout],
            graphicsDevice.MainSwapchain.Framebuffer.OutputDescription));
    }

    public override ResourceLayout TextureLayout => _textureLayout;

    public override void SetActive(CommandList commandList)
    {
        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, _viewSet);
        commandList.SetGraphicsResourceSet(1, _worldTransformSet);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _viewLayout.Dispose();
            _worldTransformLayout.Dispose();
            _textureLayout.Dispose();
            _viewSet.Dispose();
            _worldTransformSet.Dispose();
            _pipeline.Dispose();
        }

        base.Dispose(disposing);
    }
}