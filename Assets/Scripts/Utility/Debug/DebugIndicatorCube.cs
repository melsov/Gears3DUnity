using UnityEngine;
using System.Collections;

public class DebugIndicatorCube : MonoBehaviour {

    [SerializeField]
    protected Renderer _renderer;


	void Start () {
        if (!_renderer) {
            _renderer = GetComponentInChildren<Renderer>();
        }
	}
    
    public void setColor(Color color) {
        if(_renderer.material.HasProperty("_Color")) {
            _renderer.material.SetColor("_Color", color);
        }
    }
}
