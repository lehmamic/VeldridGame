namespace VeldridGame.Rendering.Shader;

public interface IBindableResourceProvider
{
    public IReadOnlyList<Uniform> Uniforms { get; }

    public bool GetBufferIndex(string id, out ushort uniformIndex, out ushort bufferIndex);
}