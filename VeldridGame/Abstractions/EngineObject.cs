namespace VeldridGame.Abstractions;

public abstract class EngineObject : IDisposable
{
    private static int _nextId = 1;

    public Game Game { get; }

    public int InstanceId { get; protected set; }

    public Guid AssetId { get; set; } = Guid.Empty;
    
    protected EngineObject(Game game)
    {
        Game = game;
        InstanceId = _nextId++;
        _nextId = Interlocked.Increment(ref _nextId);
    }
    
    ~EngineObject()
    {
        Dispose(false);
    }
    
    public static bool operator ==(EngineObject? left, EngineObject? right)
    {
        if (left is null)
        {
            return right is null;
            ; // || right.IsDestroyed;
        }

        // if (right is null)
        // {
        //     return left.IsDestroyed;
        // }
        
        return ReferenceEquals(left, right); // || (left.IsDestroyed && right.IsDestroyed);
    }

    public static bool operator !=(EngineObject? left, EngineObject? right) => !(left == right);

    public override int GetHashCode() => /*IsDestroyed ? 0 :*/ base.GetHashCode();

    public override bool Equals(object? obj) => this == (obj as EngineObject);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // TODO release managed resources here
        }
    }
}