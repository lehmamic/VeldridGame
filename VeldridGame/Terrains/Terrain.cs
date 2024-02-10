using Silk.NET.Maths;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using VeldridGame.Rendering;

namespace VeldridGame.Terrains;

public class Terrain : IDisposable
{
    private const float Size = 800.0f;
    private const float MaxHeight = 40.0f;
    private const float MaxPixelColor = 256.0f; // 256.0f * 256.0f * 256.0f; its grayscale so we use only one pixel attribute

    public Terrain(GraphicsDevice graphicsDevice, int gridX, int gridZ, string heightMap)
    {
        // TexturePack = texturePack;
        // BlendMap = blendMap;
        // X = gridX * Size;
        // Z = gridZ * Size;
        VertexArrayObject = GenerateTerrain(graphicsDevice, heightMap);
    }

    // public float X { get; }
    //
    // public float Z { get; }

    public VertexArrayObject VertexArrayObject { get; }
    
    private static VertexArrayObject GenerateTerrain(GraphicsDevice graphicsDevice, string heightMap)
    {
        var imageConfig = Configuration.Default.Clone();
        using var image = Image.Load<Rgba32>(imageConfig, heightMap);
        
        var vertexCount = image.Height;

        VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[vertexCount * vertexCount];
        int vertexPointer = 0;

        for(int x = 0; x < vertexCount; x++){
            for(int y = 0; y < vertexCount; y++){
                // position
                var position = new Vector3D<float>(
                    x / ((float)vertexCount - 1) * Size,
                    y / ((float)vertexCount - 1) * Size,
                    0); //GetHeight(y, x, image));

                // normals
                var normal = CalculateNormal( x, y, image);

                // textureCoords
                var textureCoords = new Vector2D<float>(
                    x / ((float)vertexCount - 1),
                    y / ((float)vertexCount - 1));

                vertices[vertexPointer] = new VertexPositionNormalTexture(position, normal, textureCoords);

                vertexPointer++;
            }
        }

        ushort[] indices = new ushort[6 * (vertexCount - 1) * (vertexCount - 1)];
        int indexPointer = 0;

        for(int gx = 0; gx < vertexCount - 1; gx++)
        {
            for(int gy = 0; gy < vertexCount - 1 ; gy++)
            {
                int topLeft = ((gx + 1) * vertexCount) + gy;
                int topRight = topLeft + 1;
                int bottomLeft = (gx * vertexCount) + gy;
                int bottomRight = bottomLeft + 1;

                indices[indexPointer++] = (ushort)topLeft;
                indices[indexPointer++] = (ushort)bottomLeft;
                indices[indexPointer++] = (ushort)topRight;
                indices[indexPointer++] = (ushort)topRight;
                indices[indexPointer++] = (ushort)bottomLeft;
                indices[indexPointer++] = (ushort)bottomRight;
            }
        }

        return new VertexArrayObject(graphicsDevice, vertices, indices);
    }

    private static Vector3D<float> CalculateNormal(int x, int y, Image<Rgba32> image)
    {
        float heightL = GetHeight(x - 1, y, image);
        float heightR = GetHeight(x + 1, y, image);
        float heightD = GetHeight(x, y - 1, image);
        float heightU = GetHeight(x, y + 1, image);

        var normal = new Vector3D<float>(heightL - heightR, 2.0f, heightD - heightU);
        return Vector3D.Normalize(normal);
    }

    private static float GetHeight(int x, int y, Image<Rgba32> image)
    {
        if (x < 0 || x >= image.Height || y < 0 || y >= image.Height)
        {
            return 0;
        }

        var pixelColor = image[y, x]; // x and y are flipped because of teh left handed coordinate system (x goes strait anf y goes right)
        // its grayscale so we an use only one color attribute
        float height = -1 * pixelColor.R;
        height += MaxPixelColor / 2.0f;
        height /= MaxPixelColor / 2.0f;
        height *= MaxHeight;

        return height;
    }

    public void Dispose()
    {
        VertexArrayObject.Dispose();
    }
}