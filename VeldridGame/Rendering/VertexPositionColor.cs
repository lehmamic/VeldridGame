using Silk.NET.Maths;
using Veldrid;

namespace VeldridGame.Rendering;

struct VertexPositionColor(Vector2D<float> position, RgbaFloat color)
{
    public const uint SizeInBytes = 24;

    public Vector2D<float> Position = position; // This is the position, in normalized device coordinates.
    public RgbaFloat Color = color; // This is the color of the vertex.
}