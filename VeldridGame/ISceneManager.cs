using VeldridGame.Abstractions;
using VeldridGame.Resources;

namespace VeldridGame;

public interface ISceneManager
{
    Scene Scene { get; }

    bool UpdatingActors { get; }

    void LoadScene(Scene scene);

    bool Has(Actor original);

    void Clear();
}