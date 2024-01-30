using System.Numerics;
using Veldrid;

namespace VeldridGame.Rendering;

struct VertexPositionColor(Vector2 position, RgbaFloat color)
{
    public const uint SizeInBytes = 24;

    public Vector2 Position = position; // This is the position, in normalized device coordinates.
    public RgbaFloat Color = color; // This is the color of the vertex.
}