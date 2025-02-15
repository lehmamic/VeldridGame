using Veldrid;
using Veldrid.ImageSharp;
using VeldridGame.Abstractions;

namespace VeldridGame.Rendering;

public class Texture2D : EngineObject
{
    public const string Default = "Default.png";

    private readonly Veldrid.Texture _texture;
    private readonly TextureView _textureView;
    private readonly ResourceSet _textureSet;
    private readonly ResourceLayout _textureLayout;

    public Texture2D(Game game, string path)
        : base(game)
    {
        var factory = game.Graphics.Factory;
        
        var image = new ImageSharpTexture(path);
        _texture = image.CreateDeviceTexture(game.Graphics.Device, factory);
        _textureView = factory.CreateTextureView(_texture);
        
        _textureLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));
        _textureSet = factory.CreateResourceSet(new ResourceSetDescription(_textureLayout, _textureView, game.Graphics.Device.Aniso4xSampler));
    }
    
    public uint Width => _texture.Width;
    
    public uint Height => _texture.Height;
    
    public Veldrid.Texture Texture => _texture;
    
    public void SetActive(CommandList commandList, uint slot)
    {
        commandList.SetGraphicsResourceSet(slot, _textureSet);
    }

    protected override void Dispose(bool disposing)
    {
        if(disposing)
        {
            _texture.Dispose();
            _textureView.Dispose();
            _textureLayout.Dispose();
            _textureSet.Dispose();
        }
    }
}