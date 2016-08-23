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
    public static void AlignXZDisplacePushRotation(Transform front, Vector3 back, Quaternion delta, Vector3 displace, Cog target) {
        target.move(target.transform.position + (front.position + displace - back));
        target.rotate(front.rotation * delta);
    }

    /*
     * Move target to align back with front.
     * Rotate target to match front's rotation, offset by delta
     * */
    public static void AlignXZPushRotation(Transform front, Vector3 back, Quaternion delta, Cog target) {
        target.move(target.transform.position + new VectorXZ(front.position - back).vector3());
        target.rotate(front.rotation * delta);
    }

    public static void AlignXZ(Transform child, Transform parent, Transform localOffsetObject) {
        child.transform.position = GetAlignedXZ(child, parent, localOffsetObject);
    }

    public static Vector3 GetAlignedXZ(Transform child, Transform parent, Transform localOffsetObject) {
        Vector3 localOffset = Vector3.zero;
        if (localOffsetObject != null) {
            localOffset = localOffsetObject.transform.localPosition;
            localOffset.Scale(child.transform.localScale);
            localOffset = child.transform.rotation * localOffset;
        }

        return new Vector3(
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

    public static Transform FindTagWithin(Transform parent, string tag) {
        foreach(Transform t in parent.GetComponentsInChildren<Transform>()) {
            if (t.gameObject.tag.Equals(tag)) {
                return t;
            }
        }
        return null;
    }

    public static List<Transform> FindAllWithTagWithin(Transform parent, string tag) {
        List<Transform> result = new List<Transform>();
        foreach(Transform t in parent.GetComponentsInChildren<Transform>()) {
            if (t.gameObject.tag.Equals(tag)) { result.Add(t); }
        }
        return result;
    }

    public static List<Transform> FindAllEldestGenerationWithTag(Transform root, string tag) {
        List<Transform> result = new List<Transform>();
        Queue<Transform> search = new Queue<Transform>();
        search.Enqueue(root);
        while(search.Count > 0) {
            Transform subject = search.Dequeue();
            if(subject.tag.Equals(tag)) {
                result.Add(subject);
            } else {
                foreach(Transform child in subject) {
                    search.Enqueue(child);
                }
            }
        }
        return result;
    }

    public static Transform FindChildWithTag(Transform parent, string tag) {
        foreach (Transform child in parent) {
            foreach (Transform t in child.GetComponentInChildren<Transform>()) {
                if (t.gameObject.tag.Equals(tag)) {
                    return t;
                }
            }
        }
        return null;
    }

    public static T GetComponentOnlyInChildren<T>(Transform t) where T : MonoBehaviour {
        foreach(Transform child in t) {
            T found = child.GetComponentInChildren<T>();
            if (found) { return found; }
        }
        return null;
    }

    public static T GetComponentOnlyInParents<T>(Transform t) where T : MonoBehaviour {
        return t.parent.GetComponentInParent<T>();
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

    public static T AddComponentIfNot<T>(Transform g) where T : MonoBehaviour {
        T result = g.GetComponent<T>();
        if(!result) {
            result = g.gameObject.AddComponent<T>();
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

    #region physics
    
    public static float dragCoefficient(Rigidbody rb) {
        return 1f - Time.fixedDeltaTime * rb.drag;
    }

    public static Vector3 forceForVelocity(Rigidbody rb, Vector3 targetVel) {
        return (targetVel / dragCoefficient(rb) - rb.velocity) * rb.mass / Time.fixedDeltaTime;
    }

    public static Vector3 forceForTargetPosition(Rigidbody rb, Vector3 targetPosition) {
        Vector3 dist = targetPosition - rb.position;
        Vector3 targetVel = dist / Time.fixedDeltaTime;
        return forceForVelocity(rb, targetVel);
    }

    /* 
     * Rough accuracy: ~ 1e-03 with force 1e04 and 'large' velocity  */
    public static Vector3 distanceOneFrameGiven(Rigidbody rb, Vector3 force) {
        Vector3 vel = (rb.velocity + force * Time.fixedDeltaTime / rb.mass) * dragCoefficient(rb);
        return vel * Time.deltaTime;
    }
    #endregion
}

public enum Axis
{
    X, Y, Z
};

public class AxisUtil3
{
    private delegate Vector3 SetPosition(Vector3 v, float axisPos);
    private SetPosition setPosition;
    private delegate float GetAxisPosition(Vector3 v);
    private GetAxisPosition getAxisPosition;
    public readonly Axis axis;
    
    private static SetPosition GetSetPositionFor(Axis axis) {
        SetPosition result;
        switch(axis) {
            case Axis.X:
                result = delegate (Vector3 v, float axisPos) {
                    return new Vector3(axisPos, v.y, v.z);
                };
                break;
            case Axis.Y:
                result = delegate (Vector3 v, float axisPos) {
                    return new Vector3(v.x, axisPos, v.z);
                };
                break;
            case Axis.Z:
            default:
                result = delegate (Vector3 v, float axisPos) {
                    return new Vector3(v.x, v.y, axisPos);
                };
                break;
        }
        return result;
    }

    private static GetAxisPosition GetGetPositionFor(Axis axis) {
        GetAxisPosition result;
        switch(axis) {
            case Axis.X:
                result = delegate (Vector3 v) { return v.x; };
                break;
            case Axis.Y:
                result = delegate (Vector3 v) { return v.y; };
                break;
            case Axis.Z:
            default:
                result = delegate (Vector3 v) { return v.z; };
                break;
        }
        return result;
    }

    public AxisUtil3(Axis axis_) {
        axis = axis_;
        setPosition = GetSetPositionFor(axis_);
        getAxisPosition = GetGetPositionFor(axis_);
    }

    public float axisPosition(Vector3 v) { return getAxisPosition(v); }
    
    public Vector3 positionOnAxis(Vector3 v, float axisPos) {
        return GetSetPositionFor(axis)(v, axisPos);
    }

    

}

