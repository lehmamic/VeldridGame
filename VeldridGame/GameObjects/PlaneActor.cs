using VeldridGame.Abstractions;
using VeldridGame.Rendering;

namespace VeldridGame.GameObjects;

public class PlaneActor : Actor
{
    public PlaneActor(Game game)
        : base(game)
    {
        Transform.Scale = 10.0f;
        var mesh = new MeshComponent(this);
        mesh.Mesh = Game.Renderer.GetMesh("Assets/Plane.gpmesh");
    }
}