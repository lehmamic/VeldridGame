using Veldrid;

namespace VeldridGame.Rendering.Shader;

public readonly struct Uniform(string name, ResourceKind kind, uint binding, uint size = 0) : IEquatable<Uniform>
{
    public readonly string Name = name;
    public readonly ResourceKind Kind = kind;
    public readonly uint Binding = binding;
    public readonly uint Size = size;
    
    public override bool Equals(object? obj)
    {
        if (obj is not Uniform other)
        {
            return false;
        }

        return Equals(other);
    }

    public bool Equals(Uniform other)
        => Name == other.Name;

    public override int GetHashCode()
        => Name.GetHashCode();
}