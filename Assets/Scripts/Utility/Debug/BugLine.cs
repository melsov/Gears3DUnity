using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;



public class BugLine : Singleton<BugLine> {

    protected BugLine() { }
    private LineRenderer _lineRenderer;
    private LineRenderer lineRenderer {
        get {
            if (!_lineRenderer) {
                _lineRenderer = GetComponent<LineRenderer>();
                _lineRenderer.SetWidth(.1f, .2f);
                _lineRenderer.SetVertexCount(vertices);
                // Setting all vertex positions after setting vertex count prevents the scary AABB errors
                for (int i = 0; i < vertices; ++i) {
                    _lineRenderer.SetPosition(i, Vector3.zero);
                }
            }
            return _lineRenderer;
        }
    }
    private int vertices = 25;
    private Transform _pointMarker;
    private Transform pointMarker {
        get {
            if (!_pointMarker) {
                _pointMarker = GameObject.FindGameObjectWithTag("DebugMarker").transform;
            }
            return _pointMarker;
        }
    }
    List<Transform> markers = new List<Transform>();
    Color[] colors = new Color[] {
            Color.red,
            new Color(.9f, .5f, .3f),
            Color.yellow, 
            Color.green,
            Color.blue,
            new Color(.3f, 0f, 5f),
            Color.magenta,
            Color.cyan,
            Color.gray,
            Color.black,
        };

    private Dictionary<VecPair, GameObject> lines = new Dictionary<VecPair, GameObject>();
    private bool drawOnKeyPress;

    public void Awake() {
       
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.B)) {
            drawOnKeyPress = true;
        }
        if (Input.GetKeyDown(KeyCode.C)) {
            clear();
        }
    }

    private Color colorForIndex(int i) {
        return colors[i % colors.Length];
    }

    public void drawFromOnBKeyPress(Vector3 position, Vector3 direction) {
        if (drawOnKeyPress) {
            print("drawfrom on key");
            drawOnKeyPress = false;
            drawFrom(position, direction);
        }
    }

    public void markPoint(Vector3 point, int pointIndex) {
        markPoint(new VectorXZ(point), pointIndex);
    }

    public void markPoint(VectorXZ point, int pointIndex) {
        Transform m = getMarker(pointIndex);
        m.position = point.vector3(m.position.y);
        m.GetComponent<Renderer>().material.color = colorForIndex(pointIndex);
    }

    private Transform getMarker(int index) {
        if (index < markers.Count) {
            return markers[index];
        }
        markers.Add(makeMarker());
        return markers[markers.Count - 1];
    }

    private Transform makeMarker() {
        Transform m = Instantiate<Transform>(pointMarker);
        return m;
    }

    public void drawFrom(VectorXZ origin, VectorXZ direction) {
        createAndDraw(origin.vector3(), origin.vector3() + direction.normalized.vector3());
    }

    public void drawFrom(Vector3 origin, Vector3 direction) {
        createAndDraw(origin, origin + (direction.normalized * 5f));
    }

    public void drawFromTo(Vector3 origin, Vector3 destination) {
        createAndDraw(origin, destination);
    }
    public void drawFromToBumpUp(Vector3 origin, Vector3 destination) {
        createAndDraw(origin + Vector3.up, destination + Vector3.up);
    }
    private void createAndDraw(Vector3 origin, Vector3 destination) {
        createAndDraw(origin, destination, null);
    }
    private void createAndDraw(Vector3 origin, Vector3 destination, LineRenderer lr) {
#if UNITY_EDITOR
        VecPair vp = new VecPair(origin, destination);
        if (lines.ContainsKey(vp)) {
            return;
        }
        GameObject line = this.gameObject;
        if (lr == null) {
            line = new GameObject();
            lr = line.AddComponent<LineRenderer>();
        }
        lr.SetPosition(0, origin);
        lr.SetPosition(1, destination);
        //lr.material = lineRenderer.material;

        lr.SetColors(Color.green, Color.yellow);
        lr.SetWidth(.1f, .01f);
        line.transform.SetParent(transform);
        lines.Add(vp, line);
#endif
    }

    public void circle(Vector3 origin, float radius) {
#if UNITY_EDITOR
        float sections = vertices - 1;
        float wedge = Mathf.PI * 2 / sections;
        Vector3 next;
        for(int i = 0; i <= sections; ++i) {
            float angle = i * wedge;
            next = origin + new Vector3(radius * Mathf.Cos(angle), 0f, radius * Mathf.Sin(angle));
            lineRenderer.SetPosition(i, next);
        }
#endif
    }

    public void clear() {
        foreach(GameObject li in lines.Values) {
            Destroy(li);
        }
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
