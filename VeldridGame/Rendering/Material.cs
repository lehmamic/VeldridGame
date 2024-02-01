using System.Runtime.InteropServices;

namespace VeldridGame.Rendering;

[StructLayout(LayoutKind.Sequential)]
public struct Material(float specPower)
{
    public float SpecPower = specPower;
    
#pragma warning disable 0169
    private float _padding0;
    private float _padding1;
    private float _padding2;
#pragma warning enable 0169
}