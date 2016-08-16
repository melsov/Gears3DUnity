using UnityEngine;
using System.Collections;
using System;

public class CurvyLine : MonoBehaviour {

    [SerializeField]
    protected static int increments = 16;

    [SerializeField]
    protected Color startColor = Color.magenta;
    [SerializeField]
    protected Color endColor = Color.yellow;
    private Vector3[] positions = new Vector3[increments];

    protected LineRenderer _lr;
    protected LineRenderer lr {
        get {
            if (!_lr) {
                _lr = GetComponent<LineRenderer>();
                if (!_lr) { _lr = gameObject.AddComponent<LineRenderer>(); }
                _lr.SetColors(startColor, endColor);
                _lr.SetWidth(.3f, .2f);
                _lr.SetVertexCount(increments);
            }
            return _lr;
        }
    }

    protected AnimationCurve _curve;

    public Transform start;
    public Transform end;

    private Vector3 prevStartPos;
    private Vector3 prevEndPos;

    private bool shouldRedraw() {
        if(!start.position.Equals(prevStartPos) || !end.position.Equals(prevEndPos)) {
            prevStartPos = start.position;
            prevEndPos = end.position;
            return true;
        }
        return false;
    }

    public void Awake() {
        _curve = new AnimationCurve(new Keyframe(0f, 0f, 90f, 0f), new Keyframe(1f, 1f, 0f, 90f));
    }

    public void FixedUpdate() {
        if (shouldRedraw()) {
            redraw();
        }
    }

    private VectorXZ distance { get { return end.position - start.position; } }

    private void redraw() {
        VectorXZ norm = new VectorXZ();
        for(int i = 0; i < positions.Length - 1; ++i) {
            norm.x = (float)i / positions.Length;
            norm.z = _curve.Evaluate(norm.x);
            positions[i] = start.position + (distance * norm).vector3();
        }
        positions[positions.Length - 1] = end.position;
        lr.SetPositions(positions);
    }
}
