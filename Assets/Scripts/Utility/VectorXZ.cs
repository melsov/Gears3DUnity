using UnityEngine;
using System.Collections;

public struct VectorXZ  {

    private Vector2 v;
    
    public float x {
        get { return v.x; }
        set { v.x = value; }
    }

    public float z {
        get { return v.y; }
        set { v.y = value; }
    }

    public Vector3 vector3(float y) {
        return new Vector3(v.x, y, v.y);
    }

    public Vector2 vector2 {
        get {
            return v;
        }
    }

    public float magnitudeSquared {
        get { return v.x * v.x + v.y * v.y; }
    }

    public VectorXZ(Vector3 v3) {
        v = new Vector2(v3.x, v3.z);
    }

    public VectorXZ(Vector2 v2) {
        v = v2;
    }

    public static VectorXZ operator +(VectorXZ a, VectorXZ b) {
        return new VectorXZ(a.v + b.v);
    }
    public static VectorXZ operator -(VectorXZ a, VectorXZ b) {
        return new VectorXZ(a.v - b.v);
    }

}
