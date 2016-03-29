using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;
using System.Collections;

public class TestAngleMath : ScriptableWizard {

    [MenuItem("Custom/Test Angle Math")]
    static void Test() {
        int passCount = 0;
        int posPassCount = 0;
        float tests = 120;
        foreach (float a in Angles.RadianAngles((int)tests)) {
            float ang = -Mathf.PI * 2f + a * 2f;
            VectorXZ v = Angles.UnitVectorAt(ang);
            float testAng = Angles.VectorXZToRadians(v);
            bool pass = Angles.WithinOrderOfMagnitude(testAng, ang, -5); // Angles.FloatEqual(testAng, ang);
            if (pass) { passCount++; }
            bool posPass = Angles.WithinOrderOfMagnitude(testAng, Angles.PositiveAngleRadians(ang), -5);
            if (posPass) { posPassCount++; }
            
            Assert.IsTrue(pass, "failed with ang: " + ang + "difference: " + (testAng - ang));
        }
        Debug.Log("pass rate: " + (passCount/tests));
        Debug.Log("pos pass rate: " + (posPassCount/tests));
    }

    [MenuItem("Custom/Test Float Equal")]
    static void TestEq() {
        int tests = 1000;
        int c = 0;
        for (int i = 0; i < tests; ++i) {
            float a = i * Mathf.PI - 500f * Mathf.PI;
            float b = a + Mathf.Pow(1, -300);
            bool eq = a == b;
            bool feq = Angles.FloatEqual(a, b);
            bool pass = eq == feq;
            if (pass) {
                c++;
            }
            Assert.IsTrue(pass, "Fail for : " + a);
        }
        Debug.Log("pass rate: " + c);
    }

}
