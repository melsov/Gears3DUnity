using UnityEngine;
using System.Collections;

public struct VectorXZ  {

    private Vector2 v;

    public static VectorXZ maxValue = new VectorXZ(new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));
    public static Vector3 maxVector3 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    public static VectorXZ fakeNull = new VectorXZ(float.MaxValue - 14, float.MaxValue - 6); // maxValue;
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

    public Vector3 vector3() {
        return vector3(0);
    }

    public Vector3 vector3(float y) {
        return new Vector3(v.x, y, v.y);
    }

    public Vector2 toVector2 {
        get {
            return v;
        }
    }

    public float magnitudeSquared {
        get { return v.x * v.x + v.y * v.y; }
    }

    public float magnitude {
        get { return v.magnitude; }
    }

    public VectorXZ normalized {
        get { return new VectorXZ(v.normalized); }
    }

    public VectorXZ normal {
        get {
            VectorXZ ized = normalized;
            return new VectorXZ(-ized.z, ized.x);
        }
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

    public VectorXZ(float _x, float _z) {
        v = new Vector2(_x, _z);
    }

    public float dot(VectorXZ other) {
        return Vector2.Dot(v, other.v);
    }

    public bool sympatheticDirection(VectorXZ other) {
        return dot(other) > 0f;
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
    public static VectorXZ operator *(VectorXZ a, float b) {
        return new VectorXZ(a.v * b);
    }

    public static implicit operator VectorXZ (Vector3 v) { return new VectorXZ(v); }
    public static implicit operator bool (VectorXZ v) { return !v.isFakeNull(); }

    public override bool Equals(object obj) {
        return v.Equals(obj);
    }
    public override int GetHashCode() {
        return v.GetHashCode();
    }

    public override string ToString() {
        return string.Format("VecXZ {0} , {1}", x, z);
    }

}
