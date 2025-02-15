using Silk.NET.Maths;
using Veldrid.Sdl2;

namespace VeldridGame;

public interface IScreen
{
    event EventHandler<EventArgs>? Closed;

    event EventHandler<EventArgs>? Resized;

    Sdl2Window InternalWindow { get; }

    Vector2D<int> Size { get; set; }

    int Width { get; set; }

    int Height { get; set; }

    Vector2D<int> Position { get; set; }

    void Close();
}