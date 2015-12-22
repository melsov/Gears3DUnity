using UnityEngine;
using System.Collections;

public class EnvironmentSettings : MonoBehaviour {
    public static Vector3 gravityDirection = new Vector3(0f, 0f, -1f);
    public static float gravityAmount = 9.8f;
    public static Vector3 towardsCameraDirection = new Vector3(0f, 1f, 0f);

    void Awake() {
        Physics.gravity = gravityDirection.normalized * gravityAmount;
    }
}
