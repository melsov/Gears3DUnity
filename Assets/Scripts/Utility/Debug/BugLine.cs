using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BugLine : Singleton<BugLine> {

    protected BugLine() { }

    private Dictionary<VecPair, GameObject> lines;
    void Awake() {
        lines = new Dictionary<VecPair, GameObject>();
    }

    public void drawFrom(VectorXZ origin, VectorXZ direction) {
        createAndDraw(origin.vector3(), direction.vector3());
    }

    public void drawFrom(Vector3 origin, Vector3 direction) {
        createAndDraw(origin, origin + (direction.normalized * 5f));
    }

    public void drawFromTo(Vector3 origin, Vector3 destination) {
        createAndDraw(origin, destination);
    }

    private void createAndDraw(Vector3 origin, Vector3 destination) {
        VecPair vp = new VecPair(origin, destination);
        if (lines.ContainsKey(vp)) {
            return;
        }
        GameObject line = new GameObject();
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.SetPosition(0, origin);
        lr.SetPosition(1, destination);
        lr.SetColors(Color.green, Color.yellow);
        lr.SetWidth(.4f, .2f);
        line.transform.SetParent(transform);
        lines.Add(vp, line);
    }
}

struct VecPair
{
    Vector3 a;
    Vector3 b;
    
    public VecPair(Vector3 _a, Vector3 _b) {
        a = _a; b = _b;
    }

    public override bool Equals(object obj) {
        if (obj is VecPair) {
            VecPair other = (VecPair)obj;
            return a.Equals(other.a) && b.Equals(other.b);
        }
        return false;
    }

    public override int GetHashCode() {
        return base.GetHashCode();
    }
}
