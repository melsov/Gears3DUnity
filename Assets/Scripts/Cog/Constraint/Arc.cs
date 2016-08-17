using UnityEngine;
using System.Collections;

public class Arc : MonoBehaviour {

    public Transform start;
    public Transform end;

    public Quaternion between(float interpolator) {
        return Quaternion.Slerp(start.rotation, end.rotation, Mathf.Clamp01(interpolator));
    }

    private Quaternion delta { get { return end.rotation * Quaternion.Inverse(start.rotation); } }

    private Vector3 crossWith(VectorXZ dif, bool _start) {
        VectorXZ compare = _start ? start.forward : end.forward;
        return Vector3.Cross(dif.vector3(), compare.vector3());
    }

    private bool crossIsTowardsCamera(VectorXZ dif, bool _start) {
        Vector3 cross = crossWith(dif, _start);
        return Vector3.Dot(cross, EnvironmentSettings.towardsCameraDirection) > 0f;
    }

    public float gradient(Vector3 global) {
        VectorXZ dif = global - transform.position;
        if (crossIsTowardsCamera(dif, true)) {
            dif = start.forward;
        } else if (!crossIsTowardsCamera(dif, false)) {
            dif = end.forward;
        }
        Quaternion look = Quaternion.LookRotation(dif.vector3());
        
        return gradient(look);
    }

    public float gradient(Quaternion q) {
        Quaternion fromStart = q * Quaternion.Inverse(start.rotation);
        if (delta.eulerAngles.y == 0f) { return 0f; }
        return Mathf.Clamp01(fromStart.eulerAngles.y / delta.eulerAngles.y);
    }

}
