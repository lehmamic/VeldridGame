using Veldrid;

namespace VeldridGame;

public interface IGraphics
{
    GraphicsDevice Device { get; }

    ResourceFactory Factory { get; }
    
    Framebuffer ScreenTarget { get; }
}