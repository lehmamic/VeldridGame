using Veldrid;

namespace VeldridGame.Rendering.Shader;

public sealed class ShaderVariant
{
    public VertexInput[] VertexInputs { get; set; } = [];

    public Uniform[] Uniforms { get; set; } = [];
    
    public ShaderStages[] UniformStages { get; set; } = [];

    public ShaderDescription[] GlSlShaders { get; set; } = [];
}