using Veldrid;

namespace VeldridGame.Rendering;

public static class ShaderHelper
{
    public static bool IsOpenGL(this IGraphics graphics)
    {
        return graphics.Device.BackendType == GraphicsBackend.OpenGL || graphics.Device.BackendType == GraphicsBackend.OpenGLES;
    }
    
    public static FrontFace GetFrontFace(this IGraphics graphics)
    {
        return FrontFace.CounterClockwise;
    }

    public static SpecializationConstant[] GetSpecializations(this IGraphics graphics, OutputDescription output)
    {
        var device = graphics.Device;
        
        List<SpecializationConstant> specializations =
        [
            new(100, device.IsClipSpaceYInverted),
            new(101, graphics.IsOpenGL()), // TextureCoordinatesInvertedY
            new(102, device.IsDepthRangeZeroToOne)
        ];

        PixelFormat swapchainFormat = device.MainSwapchain.Framebuffer.OutputDescription.ColorAttachments[0].Format;
        bool swapchainIsSrgb = swapchainFormat == PixelFormat.B8_G8_R8_A8_UNorm_SRgb || swapchainFormat == PixelFormat.R8_G8_B8_A8_UNorm_SRgb;
        specializations.Add(new SpecializationConstant(103, swapchainIsSrgb));

        return specializations.ToArray();
    }
}