using UnityEngine;
using System.Collections;

public class MathVector3 : MonoBehaviour {

	public static Vector3 mult(Vector3 a, Vector3 b) {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.y);
    }

    public static Vector3 div(Vector3 a, Vector3 b) {
        return new Vector3(a.x / b.x, a.y / b.y, a.z / b.y);
    }
}
