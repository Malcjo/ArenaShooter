using UnityEngine;

public static class MovementUtility
{
    public static Vector3 CamAlignedWishdir(Camera cam, Transform t, Vector2 input)
    {
        Vector3 f = cam.transform.forward; f.y = 0; f.Normalize();
        Vector3 r = cam.transform.right; r.y = 0; r.Normalize();
        return Vector3.ClampMagnitude(f * input.y + r * input.x, 1f);
    }
}
