using UnityEngine;
using System.Collections;

public class EnvironmentSettings : MonoBehaviour {
    public static Vector3 gravityDirection = new Vector3(0f, 0f, -1f);
    public static Vector3 up {
        get { return gravityDirection * -1f; }
    }

    public static Vector3 NotUp = new Vector3(1f, 1f, 0f);

    public static float gravityAmount = 9.8f;
    public static Vector3 towardsCameraDirection = new Vector3(0f, 1f, 0f);
    public static Vector3 right = new Vector3(1f, 0f, 0f);

    void Awake() {
        Physics.gravity = gravityDirection.normalized * gravityAmount;
    }
}
