using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

using UnityEngine.Assertions;
using System.Collections;



public class TestDelegatCopy : ScriptableWizard
{
    [MenuItem("Custom/Test DelegateCopy")]
    public static void test() {
        Cog.ProducerActions a = new Cog.ProducerActions();
        Cog.ProducerActions b = new Cog.ProducerActions();

        a.fulfill = delegate (Cog c) {
            MonoBehaviour.print("hi this is original a");
        };
        b.fulfill = a.fulfill;
        a.fulfill = delegate (Cog c) {
            MonoBehaviour.print("new A action");
        };

        b.fulfill(null);
    }
}

#endif