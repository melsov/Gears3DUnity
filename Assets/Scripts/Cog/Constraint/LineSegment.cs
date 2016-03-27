using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

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

    public VectorXZ closestPoint(VectorXZ global) {
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

    public bool isOnSegment(VectorXZ global) {
        VectorXZ dif = closestPointOnLine(global) - startXZ;
        return dif.dot(distance) > 0f && dif.dot(distance) < distance.magnitudeSquared;
    }

    public float slopeXZ {
        get {
            return (end.position.z - start.position.z) / (end.position.x - start.position.x);
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
        p = closestPointOnLine(p);
        if (isOnSegment(p)) { return; }
        VectorXZ dif = p - startXZ;
        if (dif.dot(distance) > 0f) { 
            end.position = p.vector3(end.position.y);
        } else { 
            start.position = p.vector3(start.position.y);
        }

        adjustSides();
        adjustedExtents();
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

    public void debug() {
        if (lr == null) { print("line rend null"); return; }
        lr.SetPosition(0, new Vector3(0, end.position.y, interceptXZ));
        float z = 100f * slopeXZ + interceptXZ;
        lr.SetPosition(1, new Vector3(100f, end.position.y, z));
        //lr.SetPosition(0, closestPoint(new VectorXZ(start.position + Vector3.right * .2f)).vector3(transform.position.y));
        //lr.SetPosition(1, closestPoint(new VectorXZ(end.position)).vector3(transform.position.y));
    }
}
