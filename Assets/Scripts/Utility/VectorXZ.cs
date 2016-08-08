using UnityEngine;
using System.Collections;

public struct VectorXZ  {

    private Vector2 v;

    public static VectorXZ maxValue = new VectorXZ(new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));
    public static Vector3 maxVector3 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    public static VectorXZ fakeNull = new VectorXZ(float.MaxValue - 14, float.MaxValue - 6); // maxValue;
    internal static VectorXZ right = new VectorXZ(1f, 0f);

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

    public VectorXZ invertedMagnitudeSafe {
        get { return  this / Mathf.Max(.01f, dot(this)); }
    }

    public static VectorXZ max(VectorXZ a, VectorXZ b) {
        return new VectorXZ(a.x > b.x ? a.x : b.x, a.z > b.z ? a.z : b.z);
    }

    public static VectorXZ min(VectorXZ a, VectorXZ b) {
        return new VectorXZ(a.x < b.x ? a.x : b.x, a.z < b.z ? a.z : b.z);
    }

    public VectorXZ sign() { return new VectorXZ(Mathf.Sign(x), Mathf.Sign(z)); }

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
    public static VectorXZ operator *(float b, VectorXZ a) {
        return new VectorXZ(a.v * b);
    }
    public static VectorXZ operator /(VectorXZ v, float f) {
        return new VectorXZ(v.x / f, v.z / f);
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

    public static bool DebugContainsNaNCheck(params VectorXZ[] vs) {
        int nans = 0;
        foreach(VectorXZ v in vs) {
            if (v.containsNaN()) { nans++; }
        }
        if (nans > 0) { Debug.Log(string.Format("{0} were NaN", nans)); }
        return nans > 0;
    }

    public bool containsNaN() {
        return float.IsNaN(x) || float.IsNaN(z);
    }

}
