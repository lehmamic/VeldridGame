using VeldridGame.Abstractions;
using VeldridGame.Input;
using VeldridGame.Resources;

namespace VeldridGame;

public class SceneManager(Game game) : ISceneManager
{
    private AssetRef<Scene> _sceneRef = new Scene(game);
    
    private bool _updatingActors = false;

    public Scene Scene => _sceneRef.Res!;
    
    public bool UpdatingActors => _updatingActors;


    public void LoadScene(Scene scene)
    {
        Clear();
        _sceneRef = scene;
    }
    
    public void Clear()
    {
        // OnSceneUnloadAttribute.Invoke();
        if (_sceneRef.Res != null)
        {
            // Camera.Main = null; // Clear the main camera so it will re-find itself and be updated

            // The act of Destroying a active scene sets the current scene to an new one
            // During this period the previous scene is Destroyed, making Res return null, hence the ? here
            _sceneRef.Res.DestroyImmediate();
            EngineObject.HandleDestroyed();

            _sceneRef = new Scene(game);
        }
    }
    
    public bool Has(Actor original)
    {
        foreach (Actor actor in Scene.Actors)
        {
            if (actor.InstanceId == original.InstanceId)
            {
                return true;
            }
        }

        return false;
    }
    
    public void ProcessInput(InputState state)
    {
        // Process input for all actors
        _updatingActors = true;
        foreach (var actor in Scene.ActiveActors)
        {
            actor.ProcessInput(state);
        }
        _updatingActors = false;
    }

    public void UpdateGame(float deltaTime)
    {
        // Update all actors
        _updatingActors = true;
        foreach (var actor in Scene.ActiveActors)
        {
            actor.Update(deltaTime);
        }
        _updatingActors = false;

        // Move any pending actors to _actors
        Scene.ActivatePendingActors();
        
        // Delete dead actors (which removes them from _actors)
        EngineObject.HandleDestroyed();
    }
}