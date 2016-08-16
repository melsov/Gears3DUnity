using UnityEngine;
using System.Collections;

public class Indicator : MonoBehaviour {

    protected Renderer _renderer;
    public Color onColor = Color.green;
    public Color offColor = Color.gray;
    public Color reverseColor = Color.red;

	void Awake () {
        awake();
	}
    protected virtual void awake() {
        _renderer = GetComponentInChildren<Renderer>();
    }

    public virtual void Start() { }
    public virtual void OnEnable() { }
    public virtual void OnDisable() { }

    protected virtual void updateIndicator() { }
}
