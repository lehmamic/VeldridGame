using VeldridGame.Input;

namespace VeldridGame.Abstractions;

public class Component : EngineObject
{
    /// <summary>
    /// Constructor. Creates an instance of the Component.
    /// </summary>
    /// <param name="actor">The owning actor.</param>
    /// <param name="updateOrder">The update order of the component. The lower the update order, the earlier the component updates. Defaults to 100.</param>
    public Component(Actor actor, int updateOrder = 100)
     : base(actor.Game)
    {
        Actor = actor;
        UpdateOrder = updateOrder;

        Actor.AddComponent(this);
    }

    /// <summary>
    /// The owning actor.
    /// </summary>
    public Actor Actor { get; }

    /// <summary>
    /// The update order of the component. The lower the update order, the earlier the component updates.
    /// </summary>
    public int UpdateOrder { get; }

    /// <summary>
    /// Update this component by delta time.
    /// </summary>
    /// <param name="deltaTime">The delta time between two frames.</param>
    public virtual void Update(float deltaTime)
    {
    }

    /// <summary>
    /// Process input for this component.
    /// </summary>
    /// <param name="state"></param>
    public virtual void ProcessInput(InputState state)
    {
    }

    /// <summary>
    /// Called when world transform changes.
    /// </summary>
    public virtual void OnUpdateWorldTransform()
    {
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Actor.RemoveComponent(this);
        }
        
        base.Dispose(disposing);
    }
}