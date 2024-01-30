using System.Numerics;

namespace VeldridGame.Rendering;

public struct VertexPosNormTex(Vector3 position, Vector3 normal, Vector2 texCoords)
{
    public const uint SizeInBytes = 8 * sizeof(float);

    public Vector3 Position = position;

    public Vector3 Normal = normal;

    public Vector2 TexCoords = texCoords;
}