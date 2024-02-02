using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace VeldridGame.Rendering;

[StructLayout(LayoutKind.Sequential)]
public struct AmbientLightInfo(Vector3D<float> color)
{
    public const uint SizeInBytes = 4 * sizeof(float);

    public Vector3D<float> Color = color;
    
#pragma warning disable 0169
    // Required to fill the uniform up to a factor of 16 bytes
    private float _padding0;
#pragma warning enable 0169
}