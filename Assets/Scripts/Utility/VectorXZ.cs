using UnityEngine;
using System.Collections;

public struct VectorXZ  {

    private Vector2 v;

    public static VectorXZ maxValue = new VectorXZ(new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));
    public static Vector3 maxVector3 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    public static VectorXZ fakeNull = maxValue;
    public bool isFakeNull() {
        return this == fakeNull;
    }

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

    public VectorXZ(float n) {
        v = new Vector2(n, n);
    }

    public static VectorXZ operator +(VectorXZ a, VectorXZ b) {
        return new VectorXZ(a.v + b.v);
    }
    public static VectorXZ operator -(VectorXZ a, VectorXZ b) {
        return new VectorXZ(a.v - b.v);
    }
    public static bool operator == (VectorXZ a, VectorXZ b) {
        return a.v == b.v;
    }
    public static bool operator != (VectorXZ a, VectorXZ b) {
        return a.v != b.v;
    }
    public override bool Equals(object obj) {
        return v.Equals(obj);
    }
    public override int GetHashCode() {
        return v.GetHashCode();
    }

}
