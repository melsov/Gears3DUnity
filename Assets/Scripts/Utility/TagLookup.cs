using UnityEngine;
using System.Collections.Generic;

public class TagLookup : MonoBehaviour {

    public static string Cog = "Cog";
    public static string ChildMesh = "ChildMesh";
    public static string Hinge = "Hinge";
    public static string MainCollider = "MainCollider";

    public static List<Transform> ChildrenWithTag(GameObject go, string tag) {
        List<Transform> result = new List<Transform>();
        foreach (Transform child in go.GetComponentsInChildren<Transform>()) {
            if (child.CompareTag(tag)) {
                result.Add(child);
            }
        }
        return result;
    }

    public static Transform UniqueChildWithTag(GameObject go, string tag) {
        List<Transform> children = ChildrenWithTag(go, tag);
        if (children.Count == 1) {
            return children[0];
        }
        return null;
    }
}
