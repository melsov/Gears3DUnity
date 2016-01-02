using UnityEngine;
using System.Collections;

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
}
