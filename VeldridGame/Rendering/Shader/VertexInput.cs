using Veldrid;

namespace VeldridGame.Rendering.Shader;

public readonly struct VertexInput(string semantic, VertexElementFormat format) : IEquatable<VertexInput>
{
    public readonly string Semantic = semantic;
    public readonly VertexElementFormat Format = format;

    public override bool Equals(object? obj)
    {
        if (obj is not VertexInput other)
        {
            return false;
        }

        return Equals(other);
    }

    public bool Equals(VertexInput other)
        => Semantic == other.Semantic;

    public override int GetHashCode()
        => Semantic.GetHashCode();
}