﻿using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class LineSegment : MonoBehaviour {

    public Transform start;
    public Transform end;

    public float fudgeStart = .01f;
    public float fudgeEnd = .05f;

    private LineRenderer lr;

    // Use this for initialization
    void Awake () {
        lr = GetComponent<LineRenderer>();
        //debug();
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

    public void debug() {
        if (lr == null) { print("line rend null"); return; }
        lr.SetPosition(0, new Vector3(0, end.position.y, interceptXZ));
        float z = 100f * slopeXZ + interceptXZ;
        lr.SetPosition(1, new Vector3(100f, end.position.y, z));
        //lr.SetPosition(0, closestPoint(new VectorXZ(start.position + Vector3.right * .2f)).vector3(transform.position.y));
        //lr.SetPosition(1, closestPoint(new VectorXZ(end.position)).vector3(transform.position.y));
    }
}
