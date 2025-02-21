using System.Numerics;
using Veldrid;
using VeldridGame.Rendering.Shader;

namespace VeldridGame.Rendering;

public sealed class CommandBuffer : IDisposable
{
    private readonly IGraphics _graphics;
    private readonly ShaderPipeline _shaderPipeline;
    private readonly BindableResourceSet _pipelineResources;
    private readonly CommandList _commandList;
    
    private bool _isRecording = false;

    private Pipeline _actualActivePipeline;

    private PolygonFillMode _fill = PolygonFillMode.Solid;
    private PrimitiveTopology _topology = PrimitiveTopology.TriangleList;
    private bool _scissor = false;
    private Framebuffer _activeFramebuffer;

    public CommandBuffer(IGraphics graphics, ShaderPipeline shaderPipeline)
    {
        _graphics = graphics;
        _shaderPipeline = shaderPipeline;
        _pipelineResources = _shaderPipeline.CreateResources();
        _commandList = _graphics.Factory.CreateCommandList();
    }
    
    ~CommandBuffer()
    {
        Dispose(false);
    }
    
    public void ActivatePipeline()
    {
        _actualActivePipeline = _shaderPipeline.GetPipeline(_fill, _topology, _scissor);
        _commandList.SetPipeline(_actualActivePipeline);
    }
    
    public void BindResources()
    {
        ResourceSet set = _pipelineResources.BindResources(_commandList);
        _commandList.SetGraphicsResourceSet(0, set);
    }
    
    public void BeginRecording()
    {
        if (!_isRecording)
        {
            _commandList.Begin();
        }

        _isRecording = true;
    }
    
    public void SetRenderTarget(Framebuffer framebuffer)
    {
        _activeFramebuffer = framebuffer;
        // _pipelineDescription.output = _activeFramebuffer.OutputDescription;

        _commandList.SetFramebuffer(framebuffer);
    }
    
    public bool SetUniform<T>(string id, T data)
        where T : unmanaged
    {
        return _pipelineResources.SetUniform(_commandList, id, data);
    }

    public void Clear()
    {
        if (_isRecording)
        {
            _commandList.End();
        }

        _isRecording = false;
    }
    
    public void SubmitCommands()
    {
        Clear();

        _graphics.Device.SubmitCommands(_commandList);
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
            _commandList.Dispose();
            _pipelineResources.Dispose();
        }
    }
    
    public void SetVertexBuffer(DeviceBuffer buffer)
    {
        _commandList.SetVertexBuffer(0, buffer);
    }

    public void SetIndexBuffer(DeviceBuffer buffer, IndexFormat format)
    {
        _commandList.SetIndexBuffer(buffer, format);
    }

    public void DrawIndexed(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
    {
        _commandList.DrawIndexed(indexCount, instanceCount, indexStart, vertexOffset, instanceStart);
    }
}