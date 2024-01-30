using System.Diagnostics;
using VeldridGame.Rendering;

namespace VeldridGame;

public class Game : IDisposable
{
    private readonly Renderer _renderer;

    public Game()
    {
        _renderer = new Renderer(1024, 768, "Veldrid Game");
    }
    
    public Renderer Renderer => _renderer;

    public void RunLoop()
    {
        var sw = Stopwatch.StartNew();
        double previousElapsed = sw.Elapsed.TotalSeconds;
        
        while (_renderer.Window.Exists)
        {
            var newElapsed = sw.Elapsed.TotalSeconds;
            float deltaTimeInSeconds = (float)(newElapsed - previousElapsed);
            
            _renderer.Window.PumpEvents();
            _renderer.Draw(deltaTimeInSeconds);
        }
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}