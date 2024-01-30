using Veldrid;

namespace VeldridGame.Rendering;

public class VertexArrayObject : IDisposable
{
    public VertexArrayObject(GraphicsDevice graphicsDevice, VertexPositionTexture[] vertices, ushort[] indices)
    {
        NumberOfVertices = vertices.Length;
        NumberOfIndices = indices.Length;

        var factory = graphicsDevice.ResourceFactory;

        VertexBuffer = factory.CreateBuffer(new BufferDescription((uint)vertices.Length * VertexPositionTexture.SizeInBytes, BufferUsage.VertexBuffer));
        graphicsDevice.UpdateBuffer(VertexBuffer, 0, vertices);

        IndexBuffer = factory.CreateBuffer(new BufferDescription((uint)indices.Length * sizeof(ushort), BufferUsage.IndexBuffer));
        graphicsDevice.UpdateBuffer(IndexBuffer, 0, indices);
    }

    public int NumberOfVertices { get; }

    public int NumberOfIndices { get; }

    public DeviceBuffer VertexBuffer { get; }

    public DeviceBuffer IndexBuffer { get; }
    
    public void SetActive(CommandList commandList)
    {
        commandList.SetVertexBuffer(0, VertexBuffer);
        commandList.SetIndexBuffer(IndexBuffer, IndexFormat.UInt16);
    }

    public void Dispose()
    {
        VertexBuffer?.Dispose();
        IndexBuffer.Dispose();
    }
}