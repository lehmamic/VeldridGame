namespace VeldridGame.Rendering;

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