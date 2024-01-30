using Silk.NET.Maths;

namespace VeldridGame.Rendering;

public struct VertexPositionNormalTexture(Vector3D<float> position, Vector3D<float> normal, Vector2D<float> texCoords)
{
    public const uint SizeInBytes = 8 * sizeof(float);

    public Vector3D<float> Position = position;

    public Vector3D<float> Normal = normal;

    public Vector2D<float> TexCoords = texCoords;
}