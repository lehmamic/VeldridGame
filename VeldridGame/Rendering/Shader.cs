using Veldrid;

namespace VeldridGame.Rendering;

public abstract class Shader : IDisposable
{
    protected readonly Dictionary<string, DeviceBuffer> BufferMap = new();

    public abstract ResourceLayout TextureLayout { get; }

    public abstract void SetActive(CommandList commandList);

    public void SetUniform<T>(CommandList commandList, string buffer, T data)
        where T : unmanaged
    {
        commandList.UpdateBuffer(BufferMap[buffer], 0, data);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var buffer in BufferMap.Values)
            {
                buffer.Dispose();
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}