using Silk.NET.Maths;
using Veldrid;
using VeldridGame.Abstractions;
using VeldridGame.Input;
using VeldridGame.Maths;

namespace VeldridGame.Camera;

public class CameraActor : Actor
{
    // private readonly MoveComponent _move;

    public CameraActor(Game game)
        : base(game)
    {
        // _move = new MoveComponent(this);
    }

    protected override void UpdateActor(float deltaTime)
    {
        base.UpdateActor(deltaTime);
        
        // Compute new camera from this actor
        Vector3D<float> cameraPosition = Transform.Position;
        Vector3D<float> target = Transform.Position + Transform.Forward * 100.0f;
        Vector3D<float> up = Vector3D<float>.UnitZ;

        Matrix4X4<float> view = GameMath.CreateLookAt(cameraPosition, target, up);
        Game.Renderer.ViewMatrix = view;
    }

    protected override void ActorInput(InputState input)
    {
        float forwardSpeed = 0.0f;
        float angularSpeed = 0.0f;

        // wasd movement
        if (input.Keyboard.GetKeyValue(Key.W))
        {
            forwardSpeed += 300.0f;
        }
    
        if (input.Keyboard.GetKeyValue(Key.S))
        {
            forwardSpeed -= 300.0f;
        }
    
        if (input.Keyboard.GetKeyValue(Key.A))
        {
            angularSpeed -= GameMath.TwoPi;
        }
    
        if (input.Keyboard.GetKeyValue(Key.D))
        {
            angularSpeed += GameMath.TwoPi;
        }

        // _move.ForwardSpeed = forwardSpeed;
        // _move.AngularSpeed = angularSpeed;
    }
}