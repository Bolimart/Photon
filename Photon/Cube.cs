using OpenTK.Mathematics;

namespace Photon;

public struct CubeData
{
    public Vector3 Position;
    public Vector3 Axis;     // axe de rotation (normalisé)
    public float Speed;      // rad/s
    public float Phase;      // angle de départ (rad)
    public float Scale;
}
