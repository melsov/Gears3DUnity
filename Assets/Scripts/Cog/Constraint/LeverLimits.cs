using UnityEngine;
using System.Collections;

public class LeverLimits : MonoBehaviour {

    protected Transform _min;
    protected Transform _max;

	void Awake () {
        foreach (Transform t in GetComponentInChildren<Transform>()) {
            if (t == transform) { continue; }
            if (_min == null) {
                _min = t;
            } else {
                if (t.position.z < _min.position.x) {
                    _max = _min;
                    _min = t;
                } else {
                    _max = t;
                }
            }
        }
	}

    public VectorXZ min { get { return new VectorXZ(_min.position); } }
    public VectorXZ max { get { return new VectorXZ(_max.position); } }

    public float distance { get { return _max.position.z - _min.position.z; } }
}
