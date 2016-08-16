using UnityEngine;
using System.Diagnostics;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
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
            print(msg);
#if UNITY_EDITOR
            EditorApplication.isPaused = true;
#endif
        }
    }
    public static bool DEBUG_SAVE_RESTORE = true;


    public static void bugSaveRestore(string msg) {
        if (DEBUG_SAVE_RESTORE) {
            print(msg);
        }
    }

    private static int tick;
    
    public static void bugLessFrequently(string msg) {
        if (tick++ > 20) {
            tick = 0;
            print(msg);
        }
    }
    public static void printCallerMethod() {
        print(callerMethod(1));
    }

    public static string getStackTrace() {
        StackTrace st = new StackTrace();
        string result = "";
        foreach(StackFrame sf in st.GetFrames()) {
            var method = sf.GetMethod();
            result = string.Format("{0} \n {1} : {2}", result,  method.DeclaringType, method.Name);
        }
        return result;
    }
    public static void stackTrace() {
        print(getStackTrace());
    }

    public static string callerMethod() {
        return callerMethod(1);
    }
    private static string callerMethod(int i) {
        System.Diagnostics.StackFrame frame = new System.Diagnostics.StackFrame(i + 1);
        var method = frame.GetMethod();
        return method.DeclaringType.ToString() + " : " + method.Name;
    }
    public static void assertNotNullPause(System.Object m) {
        assertPause(m != null, " this object, is actually null. "  + callerMethod(1));
#if UNITY_EDITOR
        EditorApplication.isPaused = m == null;
#else
        print("no editor");
#endif
    }

    public static void assertNotNullPause(MonoBehaviour m) {
        assertPause(m != null, "something is null is actually null " + callerMethod(1));
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
        if (go.GetComponentInParent<T>() != null) {
            print(s);
        }
    }

    public static void debugIfIs<T>(MonoBehaviour o, string s) {
        if (o is T) {
            print(s);
        }
    }

    public static void bugAndPause(string s) {
        UnityEngine.Debug.LogError(s);
#if UNITY_EDITOR
        EditorApplication.isPaused = true;
#endif
    }

    public static string GetCogParentName(Transform transform) {
        if (transform == null) { return "null transform"; }
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
    public static void bugIfNull(System.Object[] interactables, MonoBehaviour mb) {
        if (interactables == null) {
            print("null thing in: " + mb.name + " script: " + mb.ToString());
        }
    }
    public static void bugIfNull(System.Object[] interactables, string v) {
        if (interactables == null) {
            print(v);
        }
    }

    public static void printCogParentName(Transform t, string msg) {
        print(msg + ": " + GetCogParentName(t));
    }

    public static void bugError(string v) {
        UnityEngine.Debug.LogError(v);
    }

    private const bool DEBUG_CONTRACT = false;
    internal static void contractLog(string v) {
        if (DEBUG_CONTRACT) {
            print(v);
        }
    }
}
