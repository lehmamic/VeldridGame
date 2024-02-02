using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace VeldridGame.Rendering;

[StructLayout(LayoutKind.Sequential)]
public struct DirectionalLightInfo(Vector3D<float> direction, Vector3D<float> diffuseColor, Vector3D<float> specularColor)
{
    public const uint SizeInBytes = 12 * sizeof(float);

    /// <summary>
    /// Direction of light
    /// </summary>
    public Vector3D<float> Direction = direction;
    
#pragma warning disable 0169
    // Required to fill the uniform up to a factor of 16 bytes
    private float _padding0;
#pragma warning enable 0169

    /// <summary>
    /// Diffuse color
    /// </summary>
    public Vector3D<float> DiffuseColor = diffuseColor;
    
#pragma warning disable 0169
    // Required to fill the uniform up to a factor of 16 bytes
    private float _padding1;
#pragma warning enable 0169

    /// <summary>
    /// Specular color
    /// </summary>
    public Vector3D<float> SpecularColor = specularColor;
    
#pragma warning disable 0169
    // Required to fill the uniform up to a factor of 16 bytes
    private float _padding2;
#pragma warning enable 0169
}