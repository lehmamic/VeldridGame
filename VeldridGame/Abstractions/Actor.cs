using VeldridGame.Input;
using VeldridGame.Resources;

namespace VeldridGame.Abstractions;

public class Actor : EngineObject
{
    private readonly List<Component> _components = new();

    private readonly Transform _transform;
    
    private bool _enabled = true;

    public Actor(Scene scene)
    : base(scene.Game)
    {
        Scene = scene;
        Scene.AddActor(this);

        _transform = new(this);
    }

    public Scene Scene { get; }
    
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (value != _enabled)
            {
                _enabled = value;
                // HierarchyStateChanged();
            }
        }
    }

    public Transform Transform => _transform;
    
    public IReadOnlyList<Component> Components => _components;

    /// <summary>
    /// Update function called from Game (not overridable).
    /// </summary>
    /// <param name="deltaTime">The delta time between two frames.</param>
    public void Update(float deltaTime)
    {
        if (_enabled)
        {
            _transform.ComputeWorldTransform();

            UpdateComponents(deltaTime);
            UpdateActor(deltaTime);

            _transform.ComputeWorldTransform();
        }
    }

    /// <summary>
    /// ProcessInput function called from Game (not overridable).
    /// </summary>
    /// <param name="state"></param>
    public void ProcessInput(InputState state)
    {
        if (_enabled)
        {
            // First process input for components
            foreach (var component in _components)
            {
                component.ProcessInput(state);
            }

            ActorInput(state);
        }
    }

    public void AddComponent(Component component)
    {
        // Find the insertion point in the sorted vector
        // (The first element with a order higher than me)
        int index = 0;
        for (; index < _components.Count; index++)
        {
            if (component.UpdateOrder < _components[index].UpdateOrder)
            {
                break;
            }
        }

        // Inserts element before position of iterator
        _components.Insert(index, component);
    }
    
    public void RemoveComponent(Component component)
    {
        _components.Remove(component);
    }

    public Component? GetComponent(string type)
    {
        return _components.Find(c => string.Equals(c.GetType().Name, type, StringComparison.OrdinalIgnoreCase));
    }
    
    public Component? GetComponent<T>()
        where T : Component
    {
        return GetComponent(typeof(T));
    }
    
    public Component? GetComponent(Type type)
    {
        return _components.Find(c => c.GetType() == type);
    }

    /// <summary>
    /// Disposes the actor which will remove itself from the game and clean up all containing components.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Updates all the components attached.
    /// </summary>
    /// <param name="deltaTime">The delta time between two frames.</param>
    protected void UpdateComponents(float deltaTime)
    {
        foreach (var component in _components)
        {
            component.Update(deltaTime);
        }
    }

    /// <summary>
    /// Any actor-specific update code (overridable).
    /// </summary>
    /// <param name="deltaTime">The delta time between two frames.</param>
    protected virtual void UpdateActor(float deltaTime)
    {
    }

    /// <summary>
    /// Any actor-specific input code (overridable).
    /// </summary>
    /// <param name="state"></param>
    protected virtual void ActorInput(InputState state)
    {
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Scene.RemoveActor(this);

            // Need to delete components
            // Because ~Component calls RemoveComponent, need a different style loop
            while (_components.Any())
            {
                var component = _components.Last();
                if (component.IsDestroyed)
                {
                    continue;
                }

                component.DestroyImmediate();
            }
            
            _components.Clear();
        }
        
        base.Dispose(disposing);
    }
}