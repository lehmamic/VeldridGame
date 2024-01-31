using Silk.NET.Maths;

namespace VeldridGame.Abstractions;

public class Transform(Actor owner)
{
    private bool _recomputeWorldTransform = true;
    private Vector3D<float> _position = Vector3D<float>.Zero;
    private float _scale = 1.0f;
    private Quaternion<float> _rotation = Quaternion<float>.Identity;

    /// <summary>
    /// The owning actor.
    /// </summary>
    public Actor Owner { get; } = owner;

    public Vector3D<float> Position
    {
        get => _position;
        set
        {
            _position = value;
            _recomputeWorldTransform = true;
        }
    }

    public float Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            _recomputeWorldTransform = true;
        }
    }

    public Quaternion<float> Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            _recomputeWorldTransform = true;
        }
    }

    public Vector3D<float> Forward => Vector3D.Transform(Vector3D<float>.UnitX, _rotation);

    public Vector3D<float> Right => Vector3D.Transform(Vector3D<float>.UnitY, _rotation);

    public Matrix4X4<float> WorldTransform { get; private set; }
    
    public void RotateToNewForward(Vector3D<float> forward)
    {
        // Figure out difference between original (unit x) and new
        float dot = Vector3D.Dot(Vector3D<float>.UnitX, forward);
        float angle = Scalar.Acos(dot);
        // Facing down X
        if (dot > 0.9999f)
        {
            Rotation = Quaternion<float>.Identity;
        }
        // Facing down -X
        else if (dot < -0.9999f)
        {
            Rotation = Quaternion<float>.CreateFromAxisAngle(Vector3D<float>.UnitZ, Scalar<float>.Pi); // GameMath.CreateQuaternion(Vector3D<float>.UnitZ, Scalar<float>.Pi);
        }
        else
        {
            // Rotate about axis from cross product
            Vector3D<float> axis = Vector3D.Cross(Vector3D<float>.UnitX, forward);
            axis = Vector3D.Normalize(axis);
            Rotation = Quaternion<float>.CreateFromAxisAngle(axis, angle); //GameMath.CreateQuaternion(axis, angle);
        }
    }

    public void ComputeWorldTransform()
    {
        if (_recomputeWorldTransform)
        {
            _recomputeWorldTransform = false;

            // Scale, then rotate, then translate
            WorldTransform = Matrix4X4.CreateScale(_scale);
            WorldTransform *= Matrix4X4.CreateFromQuaternion(_rotation);
            WorldTransform *= Matrix4X4.CreateTranslation(_position);

            // Inform components world transform updated
            foreach (var component in Owner.Components)
            {
                component.OnUpdateWorldTransform();
            }
        }
    }
}