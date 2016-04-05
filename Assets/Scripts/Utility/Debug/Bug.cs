using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

public class Bug : MonoBehaviour {
    public static void printComponents(Transform g) {
        if (g == null) {
            print("BUG transform null");
            return;
        }
        printComponents(g.GetComponent<MonoBehaviour>());
    }

    public static void printComponents(GameObject g) {
        if (g == null) {
            print("BUG game object null");
            return;
        }
        printComponents(g.GetComponent<MonoBehaviour>());
    }

    public static void assertPause(bool v, string msg) {
        if (!v) {
            Debug.LogError(msg);
            EditorApplication.isPaused = true;
        }
    }

    public static void assertNotNullPause(MonoBehaviour m) {
        assertPause(m != null, " " + m.name + ", is actually null");
    }

    public static void printComponents(MonoBehaviour mb) {
        string compos = "components of: " + (mb == null ? "a null thing \n" : ( mb.name + " \n"));
        if (mb == null) {
            return;
        }
        foreach(MonoBehaviour m in mb.GetComponents<MonoBehaviour>()) {
            compos += "*" + m.name + "\n";
        }
        print(compos);
        
    }

    public static void debugIfHas<T>(GameObject go, string s) {
        if (go.GetComponent<AddOn>() != null) {
            print(s);
        }
    }

    public static void debugIfIs<T>(MonoBehaviour o, string s) {
        if (o is T) {
            print(s);
        }
    }

    public static void bugAndPause(string s) {
        Debug.LogError(s);
        EditorApplication.isPaused = true;
    }

    public static string GetCogParentName(Transform transform) {
        string result = "";
        foreach (Cog d in transform.GetComponentsInParent<Cog>()) {
            result += d.name + "_";
        }
        return result;
    }

    public static void printDrivableParentName(Transform t) { printCogParentName(t, ""); }

    internal static void bugIfNull(UnityEngine.Object t, string msg) {
        if (t == null) {
            print(msg);
        }
    }

    public static void printCogParentName(Transform t, string msg) {
        print(msg + ": " + GetCogParentName(t));
    }
}
