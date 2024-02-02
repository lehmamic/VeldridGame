using Silk.NET.Maths;
using VeldridGame.Abstractions;
using VeldridGame.Maths;

namespace VeldridGame.GameObjects;

public class MoveComponent(Actor owner, int updateOrder = 100) : Component(owner, updateOrder)
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
            Owner.Transform.Rotation = Quaternion<float>.Concatenate(Owner.Transform.Rotation, increment);
        }

        // Update position based on forward speed stays the same
        if (!ForwardSpeed.NearZero() || !StrafeSpeed.NearZero())
        {
            var pos = Owner.Transform.Position;
            pos += Owner.Transform.Forward * ForwardSpeed * deltaTime;

            // Update position based on strafe
            pos += Owner.Transform.Right * StrafeSpeed * deltaTime;

            Owner.Transform.Position = pos;
        }
    }
}