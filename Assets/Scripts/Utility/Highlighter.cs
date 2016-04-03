using UnityEngine;
using System.Collections;

public class Highlighter : MonoBehaviour {

    public Renderer _renderer;
    private Color defaultColor;
    public Color highlightColor = Color.red;

	void Awake () {
        if (_renderer == null) {
            _renderer = GetComponent<Renderer>();
        }
        if (_renderer == null) {
            _renderer = GetComponentInChildren<Renderer>();
        }
        Bug.assertPause(_renderer.materials[0] != null, "_renderer [0] is null?");
        defaultColor = material.color != null ? material.color : Color.white;
	}

    private Material material {
        get {
            return _renderer.materials[0];
        }
    }
	
    public void highlight() {
        material.color = highlightColor;
    }

    public void highlight(Color color) {
        material.color = color;
    }

    public void unhighlight() {
        material.color = defaultColor;
    }

}
