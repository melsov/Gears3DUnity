using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System;

public class TransformUtil : MonoBehaviour
{
    public static void ParentToAndAlignXZ(Transform child, Transform parent, Transform localOffsetObject) {
        AlignXZ(child, parent, localOffsetObject);
        child.transform.SetParent(parent, true);
    }

    public static void FixedJointAndAlignXZ(Rigidbody child, Rigidbody parent) {
        AlignXZ(child.transform, parent.transform, null);

        FixedJoint fj = child.GetComponent<FixedJoint>();
        if (fj == null) {
            fj = child.gameObject.AddComponent<FixedJoint>();
        }
        fj.connectedBody = parent;
    }

    public static void AlignXZ(Transform child, Transform parent, Transform localOffsetObject) {
        Vector3 localOffset = Vector3.zero;
        if (localOffsetObject != null) {
            localOffset = localOffsetObject.transform.localPosition;
            localOffset.Scale(child.transform.localScale);
            localOffset = child.transform.rotation * localOffset;
        }

        child.transform.position = new Vector3(
            -localOffset.x +
            parent.position.x,
            child.transform.position.y,
            -localOffset.z +
            parent.position.z);
    }

    public static void PositionOnYLayer(Transform trans) {
        System.Type type = typeof(Drivable);
        Drivable drivable = trans.GetComponent<Drivable>();
        if (drivable != null) {
            type = drivable.GetType();
        }
        trans.position = new Vector3(trans.position.x, YLayer.Layer(type), trans.position.z);
    }

    public static Transform FindChildWithName(Transform parent, string _name) {
        foreach(Transform child in parent.GetComponentsInChildren<Transform>()) {
            if (child.name.Equals(_name)) {
                return child;
            }
        }
        return null;
    }

    public static T FindComponentInThisOrChildren<T>(Transform t) {
        T result = t.GetComponent<T>();
        if (result == null) {
            result = t.GetComponentInChildren<T>();
        }
        return result;
    }

    public static T FindComponentInThisOrParent<T>(Transform t) {
        T result = t.GetComponent<T>();
        if (result == null) {
            result = t.GetComponentInParent<T>();
        }
        return result;
    }

    public static Vector3 SetY(Vector3 v, float y) {
        v.y = y;
        return v;
    }

    public static bool IsDescendent(Transform descendant, Transform ancestor) {
        while(descendant != null) {
            if (descendant.parent == ancestor) { return true; }
            descendant = descendant.parent;
        }
        return false;
    }

    internal static bool IsChildOf(Transform parent, Transform other) {
        if (parent == other) { return false; }
        foreach(Transform t in parent.GetComponentsInChildren<Transform>()) {
            if (t == other) { return true; }
        }
        return false;
    }
}
