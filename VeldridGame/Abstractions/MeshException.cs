namespace VeldridGame.Abstractions;

public class MeshException : Exception
{
    public MeshException()
    {
    }

    public MeshException(string? message)
        : base(message)
    {
    }
}