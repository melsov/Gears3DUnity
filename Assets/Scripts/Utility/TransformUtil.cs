using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System;
using System.Collections.Generic;

public class TransformUtil : MonoBehaviour
{
    public static void ParentToAndAlignXZ(Transform child, Transform parent, Transform localOffsetObject) {
        AlignXZ(child, parent, localOffsetObject);
        child.transform.SetParent(parent, true);
    }


    /*
     * Move target to align back with front.
     * Rotate target to match front's rotation, offset by delta
     * */
    public static void AlignXZPushRotation(Transform front, Vector3 back, Quaternion delta, Cog target) {
        target.move(target.transform.position + new VectorXZ(front.position - back).vector3());
        target.rotate(front.rotation * delta);
/* ** old way
        target.position += new VectorXZ(front.position - back).vector3();
        //TODO / CONSIDER: would it help to set the target's rotation pivot point (not here but during setup of PARENT_CHILD contracts)?
        target.rotation = front.rotation * deltaQuaternion;
*/ 
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
        Type type = typeof(Drivable);
        Drivable drivable = trans.GetComponent<Drivable>();
        if (drivable != null) {
            type = drivable.GetType();
        }
        trans.position = new Vector3(trans.position.x, YLayer.Layer(type), trans.position.z);
    }

    public static Transform FindChildWithName(Transform parent, string _name) {
        foreach(Transform child in parent) {
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

    public static List<T> FindInCogExcludingChildCogs<T>(Cog cog) {
        List<T> result = new List<T>();
        Queue<Transform> search = new Queue<Transform>();
        search.Enqueue(cog.transform);
        int safe = 0;
        while(search.Count > 0) {
            if (safe++ > 100) { Debug.LogError("hit safe limit"); break; }
            Transform trans = search.Dequeue();
            T found = trans.GetComponent<T>();
            if (found != null) {
                result.Add(found);
            }
            foreach(Transform child in trans) {
                if (!child.GetComponent<Cog>()) {
                    search.Enqueue(child);
                }
            }
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
    public static VectorXZ distanceToTangentPointAbsoluteNormalDirection(Gear gear, LineSegment lineSegment, float nudgeBeyondInnerRadius, bool flipNormal) {
        return distanceToTangentPointParallelTo(gear, lineSegment, nudgeBeyondInnerRadius, flipNormal, false);
    }
    public static VectorXZ distanceToTangentPointDeriveNormalDirection(Gear gear, LineSegment lineSegment, float nudgeBeyondInnerRadius) {
        return distanceToTangentPointParallelTo(gear, lineSegment, nudgeBeyondInnerRadius, false, true);
    }
    
    private static VectorXZ distanceToTangentPointParallelTo(Gear gear, LineSegment lineSegment, float nudgeBeyondInnerRadius, bool flipNormal, bool deriveNormalDirection) {
        VectorXZ closest = lineSegment.closestPointOnLine(new VectorXZ(gear.transform.position));
        Vector3 pos;
        if (deriveNormalDirection) {
            VectorXZ dif = closest - gear.transform.position;
            pos = gear.transform.position + dif.normalized.vector3() * (gear.innerRadius + nudgeBeyondInnerRadius);
        } else {
            pos = gear.transform.position + (lineSegment.normal * (flipNormal ? -1f : 1f)).vector3() * (gear.innerRadius + nudgeBeyondInnerRadius);
        }
        return pos - closest; 
    }
}
