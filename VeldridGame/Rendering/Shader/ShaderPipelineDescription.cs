using Veldrid;

namespace VeldridGame.Rendering.Shader;

public class ShaderPipelineDescription
{
    public ShaderVariant Shader { get; set; } = new();

    public OutputDescription? Output { get; set; }
}