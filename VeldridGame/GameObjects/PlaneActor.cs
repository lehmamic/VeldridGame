using VeldridGame.Abstractions;
using VeldridGame.Rendering;
using VeldridGame.Resources;

namespace VeldridGame.GameObjects;

public class PlaneActor : Actor
{
    public PlaneActor(Scene scene)
        : base(scene)
    {
        Transform.Scale = 10.0f;
        var mesh = new MeshComponent(this);
        mesh.Mesh = Game.Renderer.GetMesh("Assets/Plane.gpmesh");
    }
}