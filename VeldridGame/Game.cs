using VeldridGame.Rendering;

namespace VeldridGame;

public class Game : IDisposable
{
    private readonly Renderer _renderer;

    public Game()
    {
        _renderer = new Renderer(1024, 768, "Veldrid Game");
    }

    public void RunLoop()
    {
        while (_renderer.Window.Exists)
        {
            _renderer.Window.PumpEvents();
            _renderer.Draw();
        }
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}