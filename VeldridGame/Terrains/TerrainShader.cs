using Veldrid;
using Veldrid.SPIRV;
using VeldridGame.Rendering;
using Shader = VeldridGame.Rendering.Shader;

namespace VeldridGame.Terrains;

public class TerrainShader : Shader
{
    private readonly ResourceLayout _projViewLayout;
    private readonly ResourceLayout _worldTransformLayout;
    private readonly ResourceLayout _textureLayout;
    private readonly ResourceSet _projViewSet;
    private readonly ResourceSet _worldTransformSet;

    private readonly Pipeline _pipeline;

    public TerrainShader(GraphicsDevice graphicsDevice, string vertexShaderFilePath, string fragmentShaderFilePath)
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

        BufferMap[ShaderUniforms.ProjectionBuffer] = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
        BufferMap[ShaderUniforms.ViewBuffer] = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
        _projViewLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(ShaderUniforms.ProjectionBuffer, ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription(ShaderUniforms.ViewBuffer, ResourceKind.UniformBuffer, ShaderStages.Vertex)));
        _projViewSet = factory.CreateResourceSet(new ResourceSetDescription(
            _projViewLayout,
            BufferMap[ShaderUniforms.ProjectionBuffer],
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
            BlendStateDescription.SingleOverrideBlend,
            DepthStencilStateDescription.DepthOnlyLessEqual,
            RasterizerStateDescription.CullNone with { FrontFace = FrontFace.CounterClockwise, FillMode = PolygonFillMode.Wireframe},
            PrimitiveTopology.TriangleList,
            shaderSet,
            [_projViewLayout, _worldTransformLayout],
            graphicsDevice.MainSwapchain.Framebuffer.OutputDescription));
    }

    public override ResourceLayout TextureLayout => _textureLayout;

    public override void SetActive(CommandList commandList)
    {
        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, _projViewSet);
        commandList.SetGraphicsResourceSet(1, _worldTransformSet);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _projViewLayout.Dispose();
            _worldTransformLayout.Dispose();
            _textureLayout.Dispose();
            _projViewSet.Dispose();
            _worldTransformSet.Dispose();
            _pipeline.Dispose();
        }

        base.Dispose(disposing);
    }
}