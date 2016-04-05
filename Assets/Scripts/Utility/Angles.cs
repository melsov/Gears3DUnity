﻿using UnityEngine;
using System.Collections;

public class Angles {
    
    public static IEnumerable RadianAngles(int sections)
    {
        foreach(float ang in Increments(0f, Mathf.PI * 2f, sections)) {
            yield return ang;
        }
    }
    public static IEnumerable Increments(float from, float to, int sections) {
        float dif = to - from;
        for(int i = 0; i < sections; ++i) {
            yield return from + dif * i / ((float)sections);
        }
    }

    public static IEnumerable UnitVectors(float from, float to, int sections) {
        foreach(float ang in Increments(from, to, sections)) { yield return UnitVectorAt(ang); }
    }

    public static IEnumerable UnitVectors(int sections) { 
        foreach(VectorXZ v in UnitVectors(0f, Mathf.PI * 2f, sections)) { yield return v; }
    }

    public static VectorXZ UnitVectorAt(float angRadians) {
        return new VectorXZ(Mathf.Cos(angRadians), Mathf.Sin(angRadians));
    }

    public static float VectorXZToRadians(VectorXZ v) {
        return Mathf.Atan2(v.z , v.x);
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
        while(ang < 0f) { ang += 360f; }
        return ang;
    }
    public static float PositiveAngleRadians(float ang) {
        while(ang < 0f) { ang += Mathf.PI * 2; }
        return ang;
    }

    public static float FloatModSigned(float a, float mod) {
        if (VerySmall(mod)) return 0f;
        float s = Mathf.Sign(a) * Mathf.Sign(mod);
        a = Mathf.Abs(a); mod = Mathf.Abs(mod);
        return (a - Mathf.Floor(a / mod) * mod) * s;
    }
}