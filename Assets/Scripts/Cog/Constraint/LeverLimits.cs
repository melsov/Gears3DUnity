using UnityEngine;
using System.Collections;

public class LeverLimits : MonoBehaviour {

    [SerializeField]
    protected Transform _min;
    [SerializeField]
    protected Transform _max;
    [SerializeField]
    protected bool xAxisOriented; //TODO

    protected float _increments = 10f;
    public int increments {
        get { return Mathf.RoundToInt(_increments); }
        set {
            _increments = value;
        }
    }

	void Awake () {
        setupMinMax();
	}

    protected void setupMinMax() {
        foreach(Transform t in transform) {
            if (_min == null) {
                _min = t;
            } else {
                if (t.position.z < _min.position.z) {
                    _max = _min;
                    _min = t;
                } else {
                    _max = t;
                }
            }
        }
        
    }

    public VectorXZ min {
        get {
            if (!_min) {
                setupMinMax();
            }
            return new VectorXZ(_min.position);
        }
    }
    public VectorXZ max {
        get {
            if(!_max) {
                setupMinMax();
            }
            return new VectorXZ(_max.position);
        }
    }

    public float distance { get { return _max.position.z - _min.position.z; } }
    public float notch { get { return distance / _increments; } }

    public float gradientPosition(float globalZ) {
        return Mathf.Clamp(globalZ - min.z, 0f, distance);
    }

    public int closestLevel(float zPos) {
        float res = Mathf.Clamp(Mathf.RoundToInt(zPos / notch), 0, increments);
        return (int)res;
    }

    public float localZPositionForLevel(int level) {
        return level * notch;
    }
    
    public float globalZPositionForLevel(int level) {
        return min.z + localZPositionForLevel(level);
    }

    public float roundToClosestLevel(float zPos) {
        float res = localZPositionForLevel(closestLevel(zPos));
        return res;
    }
}
