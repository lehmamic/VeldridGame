using Silk.NET.Maths;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace VeldridGame;

public class Screen : IScreen
{
    public Screen(string title, Vector2D<int> size, Vector2D<int> position, WindowState initialState = WindowState.Normal)
    {
        var windowCi = new WindowCreateInfo
        {
            X = position.X,
            Y = position.Y,
            WindowWidth = size.X,
            WindowHeight = size.Y,
            WindowTitle = title,
        };
        InternalWindow = VeldridStartup.CreateWindow(ref windowCi);
        
        InternalWindow.Closed += () => Closed?.Invoke(this, EventArgs.Empty);
        InternalWindow.Resized += () => Resized?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<EventArgs>? Closed;
    
    public event EventHandler<EventArgs>? Resized;

    public Sdl2Window InternalWindow { get; }
    
    public Vector2D<int> Size
    {
        get => new(InternalWindow.Width, InternalWindow.Height);
        set { InternalWindow.Width = value.X; InternalWindow.Height = value.Y; }
    }

    public int Width
    {
        get => InternalWindow.Width;
        set => InternalWindow.Width = value;
    }

    public int Height
    {
        get => InternalWindow.Height;
        set => InternalWindow.Height = value;
    }

    public Vector2D<int> Position
    {
        get => new(InternalWindow.X, InternalWindow.Y);
        set { InternalWindow.X = value.X; InternalWindow.Y = value.Y; }
    }
    
    public void Close() => InternalWindow.Close();
}