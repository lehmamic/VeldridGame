using Silk.NET.Maths;

namespace VeldridGame.Rendering;

public struct VertexPositionTexture(Vector3D<float> pos, Vector2D<float> uv)
{
    public const uint SizeInBytes = 20;

    public float PosX = pos.X;
    public float PosY = pos.Y;
    public float PosZ = pos.Z;

    public float TexU = uv.X;
    public float TexV = uv.Y;
}