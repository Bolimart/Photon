namespace Photon;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class Camera
{
    #region - Fields and Properties

    private float _pitch;                          // radians
    private float _yaw = -MathHelper.PiOver2;      // -90° so the camera looks down -Z
    private Vector3 _front = -Vector3.UnitZ;
    private Vector3 _right = Vector3.UnitX;
    private Vector3 _up = Vector3.UnitY;

    public Vector3 Position { get; set; }
    public Vector3 Front => _front;
    public Vector3 Right => _right;
    public Vector3 Up => _up;

    public float Pitch
    {
        get => MathHelper.RadiansToDegrees(_pitch);
        set {
        _pitch = MathHelper.DegreesToRadians(MathHelper.Clamp(value, -89f, 89f));
        UpdateVectors(); 
        }
    }
    public float Yaw
    {
        get => MathHelper.RadiansToDegrees(_yaw);
        set {
            _yaw = MathHelper.DegreesToRadians(value);
            UpdateVectors(); 
        }
    }

    #endregion

    public Camera(Vector3 position)
    {
        Position = position;
        UpdateVectors();
    }
    
    public Matrix4 GetView() => Matrix4.LookAt(Position, Position + _front, _up);

    private void UpdateVectors()
    {
        _front = Vector3.Normalize(new Vector3(
            MathF.Cos(_pitch) * MathF.Cos(_yaw),
            MathF.Sin(_pitch),
            MathF.Cos(_pitch) * MathF.Sin(_yaw)));

        _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
        _up    = Vector3.Normalize(Vector3.Cross(_right, _front));
    }

}