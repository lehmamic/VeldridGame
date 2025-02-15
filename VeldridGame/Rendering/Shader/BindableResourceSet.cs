using Veldrid;

namespace VeldridGame.Rendering.Shader;

public class BindableResourceSet(
    IGraphics graphics,
    IBindableResourceProvider provider,
    ResourceSetDescription description,
    DeviceBuffer[] buffers,
    byte[][] intermediate)
{
    private readonly byte[][] _intermediate = intermediate;
    
    private ResourceSet? _resources;

    public ResourceSet BindResources(CommandList list)
    {
        foreach (Uniform uniform in provider.Uniforms)
        {
            switch (uniform.Kind)
            {
            }
        }

        if (_resources == null)
        {
            _resources = graphics.Factory.CreateResourceSet(description);
        }

        return _resources;
    }
    
    public bool SetUniform<T>(CommandList commandList, string id, T data)
        where T : unmanaged
    {
        if (!provider.GetBufferIndex(id, out ushort uniformIndex, out ushort bufferIndex))
        {
            return false;
        }

        DeviceBuffer buffer = buffers[bufferIndex];

        commandList.UpdateBuffer(buffer, 0, data);

        return true;
    }
}