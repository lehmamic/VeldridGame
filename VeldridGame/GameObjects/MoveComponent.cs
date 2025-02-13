using Silk.NET.Maths;
using VeldridGame.Abstractions;
using VeldridGame.Maths;

namespace VeldridGame.GameObjects;

public class MoveComponent(Actor actor, int updateOrder = 100) : Component(actor, updateOrder)
{
    /// <summary>
    /// Controls rotation (radians/second)
    /// </summary>
    public float AngularSpeed { get; set; }

    /// <summary>
    /// Controls forward movement
    /// </summary>
    public float ForwardSpeed { get; set; }
    
    /// <summary>
    /// Controls strafe movement
    /// </summary>
    public float StrafeSpeed { get; set; }

    public override void Update(float deltaTime)
    {
        if (!AngularSpeed.NearZero())
        {
            float angle = AngularSpeed * deltaTime;

            // Create quaternion for incremental (Rotate about up axis)
            var increment = GameMath.CreateQuaternion(Vector3D<float>.UnitZ, angle);
            
            // Concatenate old an new quaternion
            Actor.Transform.Rotation = Quaternion<float>.Concatenate(Actor.Transform.Rotation, increment);
        }

        // Update position based on forward speed stays the same
        if (!ForwardSpeed.NearZero() || !StrafeSpeed.NearZero())
        {
            var pos = Actor.Transform.Position;
            pos += Actor.Transform.Forward * ForwardSpeed * deltaTime;

            // Update position based on strafe
            pos += Actor.Transform.Right * StrafeSpeed * deltaTime;

            Actor.Transform.Position = pos;
        }
    }
}