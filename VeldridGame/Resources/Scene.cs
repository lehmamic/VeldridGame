using VeldridGame.Abstractions;
using VeldridGame.Camera;

namespace VeldridGame.Resources;

public class Scene(Game game) : EngineObject(game)
{
    private readonly HashSet<Actor> _actors = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<Actor> _pendingActors = new(ReferenceEqualityComparer.Instance);
    
    private CameraActor _cameraActor = null!;

    public IEnumerable<Actor> Actors => _actors.Where(a => a is { IsDestroyed: false });

    public void AddActor(Actor actor)
    {
        // If updating actors, need to add to pending
        if (Game.SceneManager.UpdatingActors)
        {
            _pendingActors.Add(actor);
        }
        else
        {
            _actors.Add(actor);
        }
    }
    
    public void RemoveActor(Actor actor)
    {
        // Is it in pending actors?
        _pendingActors.Remove(actor);

        // Is it in actors?
        _actors.Remove(actor);
    }

    public void ActivatePendingActors()
    {
        // Move any pending actors to _actors
        foreach (var pending in _pendingActors)
        {
            pending.Transform.ComputeWorldTransform();
            _actors.Add(pending);
        }

        _pendingActors.Clear();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Delete actors
            // Because ~Actor calls RemoveActor, have to use a different style loop
            foreach (var actor in _actors.ToArray())
            {
                actor.Dispose();
            }
        }
        
        base.Dispose(disposing);
    }
}