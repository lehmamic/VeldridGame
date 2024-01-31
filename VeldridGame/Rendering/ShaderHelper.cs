using Veldrid;

namespace VeldridGame.Rendering;

public static class ShaderHelper
{
    public static SpecializationConstant[] GetSpecializations(GraphicsDevice gd)
    {
        bool glOrGles = gd.BackendType == GraphicsBackend.OpenGL || gd.BackendType == GraphicsBackend.OpenGLES;

        List<SpecializationConstant> specializations =
        [
            new SpecializationConstant(100, gd.IsClipSpaceYInverted),
            new SpecializationConstant(101, glOrGles), // TextureCoordinatesInvertedY
            new SpecializationConstant(102, gd.IsDepthRangeZeroToOne)
        ];

        PixelFormat swapchainFormat = gd.MainSwapchain.Framebuffer.OutputDescription.ColorAttachments[0].Format;
        bool swapchainIsSrgb = swapchainFormat == PixelFormat.B8_G8_R8_A8_UNorm_SRgb || swapchainFormat == PixelFormat.R8_G8_B8_A8_UNorm_SRgb;
        specializations.Add(new SpecializationConstant(103, swapchainIsSrgb));

        return specializations.ToArray();
    }
}