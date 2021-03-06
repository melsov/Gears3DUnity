﻿using UnityEngine;
using System.Collections;

public class Angles {

    public static IEnumerable RadianAngles(int sections) {
        foreach (float ang in Increments(0f, Mathf.PI * 2f, sections)) {
            yield return ang;
        }
    }
    public static IEnumerable Increments(float from, float to, int sections) {
        float dif = to - from;
        for (int i = 0; i < sections; ++i) {
            yield return from + dif * i / ((float)sections);
        }
    }

    public static IEnumerable UnitVectors(float from, float to, int sections) {
        foreach (float ang in Increments(from, to, sections)) { yield return UnitVectorAt(ang); }
    }

    public static IEnumerable UnitVectors(int sections) {
        foreach (VectorXZ v in UnitVectors(0f, Mathf.PI * 2f, sections)) { yield return v; }
    }

    public static VectorXZ UnitVectorAt(float angRadians) {
        return Quaternion.Euler(new Vector3(0f, Mathf.Rad2Deg * angRadians, 0f)) * Vector3.right;// new VectorXZ(Mathf.Cos(angRadians), Mathf.Sin(angRadians));
    }

    public static Quaternion rightPointsTowards(VectorXZ rel) {
        return Quaternion.Euler(new Vector3(0f, -Angles.VectorXZToDegrees(rel), 0f));
    }

    public static Quaternion zPosPointsTowards(VectorXZ rel) {
        return Quaternion.Euler(new Vector3(0f, -Angles.VectorXZToDegrees(rel) + 90f, 0f));
    }

    public static float VectorXZToRadians(VectorXZ v) {
        return Mathf.Atan2(v.z, v.x);
    }

    public static float VectorXZToDegrees(VectorXZ v) { return Mathf.Rad2Deg * VectorXZToRadians(v); }

    public static bool WithinOrderOfMagnitude(float a, float b, float mag) {
        return Mathf.Abs(a - b) < Mathf.Pow(1f, mag);
    }
    public static bool FloatEqual(float a, float b) {
        return Mathf.Abs(a - b) < Mathf.Epsilon;
    }
    public static bool VerySmall(float a) { return FloatEqual(a, 0f); }

    public static float PositiveAngleDegrees(float ang) {
        while (ang < 0f) { ang += 360f; }
        return ang;
    }
    public static float PositiveAngleRadians(float ang) {
        while (ang < 0f) { ang += Mathf.PI * 2; }
        return ang;
    }

    public static float FloatModSigned(float a, float mod) {
        if (VerySmall(mod)) return 0f;
        float s = Mathf.Sign(a) * Mathf.Sign(mod);
        a = Mathf.Abs(a); mod = Mathf.Abs(mod);
        return (a - Mathf.Floor(a / mod) * mod) * s;
    }

    public static float adjustedDeltaAngleDegrees(float delta) {
        if (Mathf.Abs(delta) < 180f) { // CONSIDER: limits angVelocity to < 180. Is this OK?
            return delta;
        }
        //Corrects delta where angle has jumped over 360 limit
        return delta + -1f * Mathf.Sign(delta) * 360f;
    }

    /* Clockwise/positive-degrees tangent when camera is looking towards negative y */
    public static VectorXZ positiveRotationTangent(VectorXZ radius) {
        return Vector3.Cross(radius.normalized.vector3(), EnvironmentSettings.towardsCameraDirection * -1f);
    }

    public static Vector3 radialVectorsToTorqueXZ(VectorXZ from, VectorXZ to, Rigidbody target, float intervalSeconds) {
        return radialVectorsToTorque(from.vector3(), to.vector3(), target, intervalSeconds);
    }
    
    /* credit: answers.unity3d.com/questions/48836/determining-the-torque-needed-to-rotate-an-object.html */
    public static Vector3 radialVectorsToTorque(Vector3 from, Vector3 to, Rigidbody target, float intervalSeconds) {
        Vector3 x = Vector3.Cross(from.normalized, to.normalized);
        float theta = Mathf.Asin(x.magnitude);
        Vector3 w = x.normalized * theta / intervalSeconds;
        Quaternion q = target.rotation * target.inertiaTensorRotation;
        return q * Vector3.Scale(target.inertiaTensor, Quaternion.Inverse(q) * w);
    }

    public static bool containsNaN(Vector3 v) {
        return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);
    }
}
