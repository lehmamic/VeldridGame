using System.Numerics;

namespace VeldridGame.Rendering;

public struct VertexPositionTexture(Vector3 pos, Vector2 uv)
{
    public const uint SizeInBytes = 20;

    public float PosX = pos.X;
    public float PosY = pos.Y;
    public float PosZ = pos.Z;

    public float TexU = uv.X;
    public float TexV = uv.Y;
}