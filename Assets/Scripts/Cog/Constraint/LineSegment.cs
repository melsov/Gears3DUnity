using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;

public class LineSegment : MonoBehaviour {

    public Transform start;
    public Transform end;
    protected List<Transform> sides = new List<Transform>(2);

    public float fudgeStart = .01f;
    public float fudgeEnd = .05f;

    private LineRenderer lr;
    private Vector3 originalDistance;
    private Vector3 originalStartLocal;

    public delegate void AdjustedExtents();
    public AdjustedExtents adjustedExtents;

    internal void setEndPosition(VectorXZ global) {
        end.position = new Vector3(global.x, end.position.y, global.z);
    }

    internal void setStartPosition(VectorXZ global) {
        start.position = new Vector3(global.x, start.position.y, global.z);
    }

    internal VectorXZ midPoint() {
        return startXZ + distance * .5f;
    }

    internal VectorXZ closestTerminus(VectorXZ global) {
        return (global - startXZ).magnitudeSquared < (global - endXZ).magnitudeSquared ? startXZ : endXZ;
    }

    // Use this for initialization
    void Awake () {
        lr = GetComponent<LineRenderer>();
        originalDistance = end.position - start.position;
        originalStartLocal = start.localPosition;

        foreach(BoxCollider bc in GetComponentsInChildren<BoxCollider>()) {
            Transform t = bc.transform;
            if (t == transform || t == start || t == end) { continue; }
            sides.Add(t);
        }
	}

    public VectorXZ distance {
        get { return new VectorXZ(end.position - start.position); }
    }

    public VectorXZ normalized {
        get { return new VectorXZ(end.position - start.position).normalized; }
    }

    public VectorXZ normal {
        get { return normalized.normal; }
    }

    public VectorXZ startXZ {
        get { return new VectorXZ(start.position); }
    }

    public VectorXZ endXZ {
        get { return new VectorXZ(end.position); }
    }

    public VectorXZ closestPointOnLine(VectorXZ global) {
        return startXZ + normalized * normalized.dot(global - startXZ);
    }

    public float axisPosition(VectorXZ global) {
        return dotWithNormalized(global - startXZ);
    }

    public float dotWithNormalized(VectorXZ v) {
        return v.dot(normalized);
    }

    public VectorXZ closestPointOnSegment(VectorXZ global) {
        VectorXZ result = closestPointOnLine(global);
        VectorXZ dif = result - startXZ;
        // landed behind start?
        if (dif.dot(distance) < 0f) {
            return startXZ;
        }
        // beyond end?
        if (dif.dot(distance) > distance.magnitudeSquared) {
            return endXZ;
        }
        return result;
    }

    public bool sympatheticDirection(VectorXZ dir) {
        return distance.dot(dir) > 0f;
    }

    public Vector3 sympatheticToNormal(Vector3 v) {
        return sympatheticToNormal((VectorXZ)v).vector3(v.y);
    }

    public VectorXZ sympatheticToNormal(VectorXZ v) {
        VectorXZ norm = normal;
        if (norm.dot(v) > 0) return v;
        return (v - 2f * (norm * v.dot(norm)));
    }

    public bool withinSegmentDomain(VectorXZ global) {
        return sympatheticDirection(global - startXZ) && !sympatheticDirection(global - endXZ);
        /* pepperoni
         * 
        VectorXZ dif = closestPointOnLine(global) - startXZ;
        return dif.dot(distance) > 0f && dif.dot(distance) < distance.magnitudeSquared;
        */
    }

    public bool isOnSegment(VectorXZ global) {
        return isOnSegment(global, .05f);
    }
    
    public bool isOnSegment(VectorXZ global, float tolerance) {
        VectorXZ fromStart = global - startXZ;
        return withinSegmentDomain(global) && Mathf.Abs(fromStart.slope - slopeXZ) < tolerance;
    }

    public float slopeXZ {
        get {
            return distance.slope;// (end.position.z - start.position.z) / (end.position.x - start.position.x);
        }
    }

    public float interceptXZ {
        get {
            return end.position.z - slopeXZ * end.position.x;
        } 
    }

    public void setDistance(float distance) {
        end.position = start.position + (normalized * distance).vector3();
    }

    public void extendToAccommodate(VectorXZ p)
    {
        try {
            if (p == default(VectorXZ)) { return; }
            p = closestPointOnLine(p);
            if (withinSegmentDomain(p)) { return; }
            VectorXZ dif = p - startXZ;
            if (dif.dot(distance) > 0f) {
                end.position = p.vector3(end.position.y);
            } else {
                start.position = p.vector3(start.position.y);
            }

            adjustSides();
            adjustedExtents();
        } catch(System.NullReferenceException nre) {
            Debug.LogError("caught null ref exception in extend to accommodate \n" + nre.StackTrace);
            print("vec xz questionable? x: " + p.x + ", z: " + p.z);
        }
    }

    public void resetExtents() {
        start.localPosition = originalStartLocal;
        setDistance(originalDistance.magnitude);
        adjustSides();
        adjustedExtents();
    }

    public void setExtents(VectorXZ a, VectorXZ b) {
        throw new System.Exception(); // not in use
        if ((b - a).dot(distance) < 0f) {
            VectorXZ temp = a;
            a = b;
            b = temp;
        }
        start.position = a.vector3(start.position.y);
        end.position = b.vector3(end.position.y);
        adjustSides();
        adjustedExtents();
    }


    private void adjustSides() {
        float sideScale = distance.magnitude;
        foreach(Transform side in sides) {
            side.localScale = new Vector3(sideScale, side.localScale.y, side.localScale.z);
            side.localPosition = new Vector3(start.localPosition.x + sideScale / 2f, side.localPosition.y, side.localPosition.z);
        }
    }

    #region mini-serialize
    [System.Serializable]
    class SerializeStorage
    {
        public SerializableVector3 start;
        public SerializableVector3 end;
    }
    public void MiniSerialize(ref List<byte[]> data) {
        SerializeStorage stor = new SerializeStorage();
        stor.start = start.position;
        stor.end = end.position;
        SaveManager.Instance.SerializeIntoArray(stor, ref data);
    }

    public void MiniDeserialize(ref List<byte[]> data) {
        SerializeStorage stor;
        if ((stor = SaveManager.Instance.DeserializeFromArray<SerializeStorage>(ref data)) != null) {
            start.position = stor.start;
            end.position = stor.end;
            adjustSides();
            adjustedExtents();
        }
    }
    #endregion

    public void debug() {
        if (lr == null) { print("line rend null"); return; }
        lr.SetPosition(0, new Vector3(0, end.position.y, interceptXZ));
        float z = 100f * slopeXZ + interceptXZ;
        lr.SetPosition(1, new Vector3(100f, end.position.y, z));
        //lr.SetPosition(0, closestPoint(new VectorXZ(start.position + Vector3.right * .2f)).vector3(transform.position.y));
        //lr.SetPosition(1, closestPoint(new VectorXZ(end.position)).vector3(transform.position.y));
    }

}
