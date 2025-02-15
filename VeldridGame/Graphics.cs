using Veldrid;
using Veldrid.StartupUtilities;

namespace VeldridGame;

public class Graphics : IGraphics, IDisposable
{
    private readonly IScreen _screen;

    public Graphics(IScreen screen, bool vSync = true, GraphicsBackend preferredBackend = GraphicsBackend.OpenGL)
    {
        _screen = screen;
        var options = new GraphicsDeviceOptions(
            debug: true,
            swapchainDepthFormat: PixelFormat.R16_UNorm,
            syncToVerticalBlank: vSync,
            resourceBindingModel: ResourceBindingModel.Improved,
            preferDepthRangeZeroToOne: true,
            preferStandardClipSpaceYDirection: true);

        Device = VeldridStartup.CreateGraphicsDevice(_screen.InternalWindow, options, preferredBackend);
    }
    
    public GraphicsDevice Device { get; }

    public ResourceFactory Factory => Device.ResourceFactory;

    public Framebuffer ScreenTarget => Device.SwapchainFramebuffer;

    public void Dispose()
    {
        Device.Dispose();
    }
}