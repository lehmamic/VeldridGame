using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace VeldridGame.Camera;

[StructLayout(LayoutKind.Sequential)]
public struct CameraInfo(Vector3D<float> position)
{
    public const uint SizeInBytes = 4 * sizeof(float);
    
    public Vector3D<float> Position = position;

#pragma warning disable 0169
    // Required to fill the uniform up to a factor of 16 bytes
    private float _padding0;
#pragma warning enable 0169
}