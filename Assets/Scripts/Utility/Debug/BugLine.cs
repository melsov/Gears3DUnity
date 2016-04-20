using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;



public class BugLine : Singleton<BugLine> {

    protected BugLine() { }

    private LineRenderer lineRenderer;
    private int vertices = 25;
    Transform pointMarker;
    List<Transform> markers;
    Color[] colors;

    private Dictionary<VecPair, GameObject> lines;
    private bool drawOnKeyPress;

    void Awake() {
        colors = new Color[] {
            Color.red,
            Color.green,
            Color.blue,
            Color.cyan,
            Color.yellow
        };
        lines = new Dictionary<VecPair, GameObject>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetWidth(.1f, .2f);
        lineRenderer.SetVertexCount(vertices);
        // Setting all vertex positions after setting vertex count prevents the scary AABB errors
        for(int i = 0; i < vertices; ++i) {
            lineRenderer.SetPosition(i, Vector3.zero);
        }
        pointMarker = GameObject.FindGameObjectWithTag("DebugMarker").transform;
        markers = new List<Transform>();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.B)) {
            drawOnKeyPress = true;
        }
    }

    private Color colorForIndex(int i) {
        return colors[i % colors.Length];
    }

    public void drawFromOnKey(Vector3 position, Vector3 direction) {
        if (drawOnKeyPress) {
            print("drawfrom on key");
            drawOnKeyPress = false;
            drawFrom(position, direction);
        }
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
        createAndDraw(origin.vector3(), direction.vector3());
    }

    public void drawFrom(Vector3 origin, Vector3 direction) {
        createAndDraw(origin, origin + (direction.normalized * 5f));
    }

    public void drawFromTo(Vector3 origin, Vector3 destination) {
        createAndDraw(origin, destination);
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
