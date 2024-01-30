using Silk.NET.Maths;

namespace VeldridGame.Rendering;

public struct VertexPositionNormalSkinTexture(
    Vector3D<float> position,
    Vector3D<float> normal,
    Vector4D<byte> skinningIndices,
    Vector4D<byte> skinningWeights,
    Vector2D<float> texCoords)
{
    public const uint SizeInBytes = 8 * sizeof(float) + 8 * sizeof(byte);

    public Vector3D<float> Position = position;

    public Vector3D<float> Normal = normal;

    public Vector4D<byte> SkinningIndices = skinningIndices;

    public Vector4D<byte> SkinningWeights = skinningWeights;

    public Vector2D<float> TexCoords = texCoords;
}