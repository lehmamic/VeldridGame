using Veldrid;
using Veldrid.ImageSharp;

namespace VeldridGame.Rendering;

public class Texture : IDisposable
{
    private readonly Veldrid.Texture _texture;
    private readonly TextureView _textureView;
    private readonly ResourceSet _textureSet;

    public Texture(GraphicsDevice graphicsDevice, ResourceLayout textureLayout, string path)
    {
        var factory = graphicsDevice.ResourceFactory;
        
        var image = new ImageSharpTexture(path);
        _texture = image.CreateDeviceTexture(graphicsDevice, factory);
        _textureView = factory.CreateTextureView(_texture);
        _textureSet = factory.CreateResourceSet(new ResourceSetDescription(textureLayout, _textureView, graphicsDevice.Aniso4xSampler));
    }
    
    public void SetActive(CommandList commandList, uint slot)
    {
        commandList.SetGraphicsResourceSet(slot, _textureSet);
    }

    public void Dispose()
    {
        _texture.Dispose();
        _textureView.Dispose();
        _textureSet.Dispose();
    }
}