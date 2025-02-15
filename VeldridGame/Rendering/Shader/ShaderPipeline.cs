using Veldrid;
using Veldrid.SPIRV;
using VeldridGame.Assets;

namespace VeldridGame.Rendering.Shader;

public sealed class ShaderPipeline : IBindableResourceProvider, IDisposable
{
    private const int PipelineCount = 20; // 20 possible combinations (5 topologies, 2 fill modes, 2 scissor modes)
    
    private readonly IGraphics _graphics;
    private readonly IAssetProvider _assetProvider;
    private readonly ShaderVariant _shader;
    private readonly ShaderSetDescription _shaderSet;
    private readonly ResourceLayout _resourceLayout;
    private readonly Pipeline?[] _pipelines;

    private readonly Dictionary<string, uint> _bufferLookup = new();
    private readonly byte _bufferCount;

    private GraphicsPipelineDescription _description;

    public ShaderPipeline(IGraphics graphics, IAssetProvider assetProvider, ShaderPipelineDescription description)
    {
        _graphics = graphics;
        _assetProvider = assetProvider;
        _shader = description.Shader;
        _description = new GraphicsPipelineDescription();
        
        var output = description.Output ?? _graphics.ScreenTarget.OutputDescription;

        _shaderSet = CreateShaderSetDescription(output);

        // Create resource layout and uniform lookups
        var layoutDescription = new ResourceLayoutDescription(new ResourceLayoutElementDescription[_shader.Uniforms.Length]);

        for (ushort uniformIndex = 0; uniformIndex < _shader.Uniforms.Length; uniformIndex++)
        {
            Uniform uniform = _shader.Uniforms[uniformIndex];
            ShaderStages stages = _shader.UniformStages[uniformIndex];

            layoutDescription.Elements[uniform.Binding] = new ResourceLayoutElementDescription(uniform.Name, uniform.Kind, stages);

            if (uniform.Kind != ResourceKind.UniformBuffer)
            {
                continue;
            }

            _bufferLookup[uniform.Name] = Pack(uniformIndex, _bufferCount);
            _bufferCount++;
        }

        _resourceLayout = _graphics.Factory.CreateResourceLayout(layoutDescription);

        var rasterizerState = RasterizerStateDescription.CullNone with
        {
            FrontFace = _graphics.GetFrontFace(),
        };
        
        _pipelines = new Pipeline[PipelineCount];
        
        _description = new(
            BlendStateDescription.SingleOverrideBlend,
            DepthStencilStateDescription.DepthOnlyLessEqual,
            rasterizerState,
            PrimitiveTopology.TriangleList,
            _shaderSet,
            [_resourceLayout],
            output);
    }
    
    ~ShaderPipeline()
    {
        Dispose(false);
    }
    
    public IReadOnlyList<Uniform> Uniforms => _shader.Uniforms;

    public Pipeline GetPipeline(PolygonFillMode fill, PrimitiveTopology topology, bool scissor)
    {
        int index = (int)topology * 4 + (int)fill * 2 + (scissor ? 0 : 1);

        if (_pipelines[index] == null)
        {
            _description.RasterizerState.ScissorTestEnabled = scissor;
            _description.RasterizerState.FillMode = fill;
            _description.PrimitiveTopology = topology;

            _pipelines[index] = _graphics.Factory.CreateGraphicsPipeline(_description);
        }

        return _pipelines[index]!;
    }

    public BindableResourceSet CreateResources()
    {
        BindableResource[] boundResources = new BindableResource[_shader.Uniforms.Length];

        DeviceBuffer[] boundBuffers = new DeviceBuffer[_bufferCount];
        byte[][] intermediateBuffers = new byte[_bufferCount][];

        for (int i = 0, b = 0; i < _shader.Uniforms.Length; i++)
        {
            boundResources[_shader.Uniforms[i].Binding] = GetBindableResource(_shader.Uniforms[i], out DeviceBuffer? buffer);

            if (buffer != null)
            {
                boundBuffers[b] = buffer;
                intermediateBuffers[b] = new byte[buffer.SizeInBytes];

                b++;
            }
        }

        ResourceSetDescription setDescription = new ResourceSetDescription(_resourceLayout, boundResources);
        BindableResourceSet resources = new BindableResourceSet(_graphics, this, setDescription, boundBuffers, intermediateBuffers);

        return resources;
    }

    public bool GetBufferIndex(string name, out ushort uniform, out ushort buffer)
    {
        uniform = 0;
        buffer = 0;

        if (_bufferLookup.TryGetValue(name, out uint packed))
        {
            Unpack(packed, out uniform, out buffer);
            return true;
        }

        return false;
    }

    private static uint Pack(ushort a, ushort b)
        => ((uint)a << 16) | b;

    private static void Unpack(uint packed, out ushort a, out ushort b)
        => (a, b) = ((ushort)(packed >> 16), (ushort)(packed & ushort.MaxValue));

    private ShaderSetDescription CreateShaderSetDescription(OutputDescription output)
    {
        ShaderDescription[] shaderDescriptions = _shader.GlSlShaders;

        Veldrid.Shader[] shaders = _graphics.Factory.CreateFromSpirv(
            shaderDescriptions.Single(s => s.Stage == ShaderStages.Vertex),
            shaderDescriptions.Single(s => s.Stage == ShaderStages.Fragment));
        
        var vertexLayouts = new VertexLayoutDescription[_shader.VertexInputs.Length];
        for (int inputIndex = 0; inputIndex < vertexLayouts.Length; inputIndex++)
        {
            VertexInput input = _shader.VertexInputs[inputIndex];

            vertexLayouts[inputIndex] = new VertexLayoutDescription(
                new VertexElementDescription(input.Semantic, VertexElementSemantic.TextureCoordinate, input.Format));
        }
        
        return new ShaderSetDescription(vertexLayouts, shaders, _graphics.GetSpecializations(output));
    }
    
    private BindableResource GetBindableResource(Uniform uniform, out DeviceBuffer? buffer)
    {
        buffer = null;

        switch (uniform.Kind)
        {
            case ResourceKind.TextureReadOnly:
                return _assetProvider.LoadAsset<Texture2D>(Texture2D.Default).Res.Texture;
            //
            // if (uniform.Kind == ResourceKind.TextureReadWrite)
            // {
            //     return Texture2D.EmptyRW.Res.InternalTexture;
            // }
            case ResourceKind.Sampler:
                return _graphics.Device.Aniso4xSampler;
            // if (uniform.Kind == ResourceKind.StructuredBufferReadOnly)
            // {
            //     return GraphicsBuffer.Empty.Buffer;
            // }
            //
            // if (uniform.kind == ResourceKind.StructuredBufferReadWrite)
            // {
            //     return GraphicsBuffer.EmptyRW.Buffer;
            // }
            case ResourceKind.UniformBuffer:
            {
                uint bufferSize = uniform.Size;
                buffer = _graphics.Factory.CreateBuffer(new BufferDescription(bufferSize, BufferUsage.UniformBuffer));
                break;
            }

            default:
                throw new InvalidOperationException($"Resource kind {uniform.Kind} is unknown.");
        }

        return buffer;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            for (int i = 0; i < _shaderSet.Shaders.Length; i++)
            {
                _shaderSet.Shaders[i]?.Dispose();
            }

            for (int i = 0; i < _pipelines.Length; i++)
            {
                _pipelines[i]?.Dispose();
            }

            _resourceLayout?.Dispose();
        }
    }
}