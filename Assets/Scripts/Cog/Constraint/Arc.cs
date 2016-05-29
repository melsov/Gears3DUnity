using UnityEngine;
using System.Collections;

public class Arc : MonoBehaviour {

    public Transform start;
    public Transform end;

    public Quaternion between(float interpolator) {
        return Quaternion.Slerp(start.rotation, end.rotation, Mathf.Clamp01(interpolator));
    }

}
