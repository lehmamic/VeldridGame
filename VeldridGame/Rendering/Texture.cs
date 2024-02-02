using Veldrid;
using Veldrid.ImageSharp;

namespace VeldridGame.Rendering;

public class Texture : IDisposable
{
    private readonly Veldrid.Texture _texture;
    private readonly TextureView _textureView;
    private readonly ResourceSet _textureSet;
    private readonly ResourceLayout _textureLayout;

    public Texture(GraphicsDevice graphicsDevice, string path)
    {
        var factory = graphicsDevice.ResourceFactory;
        
        var image = new ImageSharpTexture(path);
        _texture = image.CreateDeviceTexture(graphicsDevice, factory);
        _textureView = factory.CreateTextureView(_texture);
        
        _textureLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));
        _textureSet = factory.CreateResourceSet(new ResourceSetDescription(_textureLayout, _textureView, graphicsDevice.Aniso4xSampler));
    }
    
    public uint Width => _texture.Width;
    
    public uint Height => _texture.Height;
    
    public void SetActive(CommandList commandList, uint slot)
    {
        commandList.SetGraphicsResourceSet(slot, _textureSet);
    }

    public void Dispose()
    {
        _texture.Dispose();
        _textureView.Dispose();
        _textureLayout.Dispose();
        _textureSet.Dispose();
    }
}