using UnityEngine;
using System.Collections.Generic;

public class Highlightable : MonoBehaviour {
    [SerializeField]
    protected Renderer _renderer;
    protected Material defaultMaterial;
    [SerializeField]
    protected Material highlightMaterial;
    private GameObject highlightMesh;
    public Color highlightColor = new Color(.1f, 1f, 1f, 1f);
    protected Color defaultColor;

	void Awake () {
        if (highlightMaterial == null) {
            foreach(Material m in Resources.LoadAll<Material>("Materials")) {
                if (m.name.Equals("Highlight")) {
                    highlightMaterial = m;
                    break;
                }
            }
        }
        Bug.assertNotNullPause(highlightMaterial);
        foreach (Transform t in GetComponentInChildren<Transform>()) {
            if (t.CompareTag("HighlightMesh")) {
                highlightMesh = t.gameObject;
                highlightMesh.SetActive(false);
                break;
            }
        }
        if (_renderer == null) {
            _renderer = GetComponent<Renderer>();
        }
        Bug.assertPause(_renderer.materials[0] != null, "_renderer [0] is null?");
        defaultMaterial = _renderer.material;
        if (defaultMaterial.HasProperty("_Color")) {
            defaultColor = defaultMaterial.color;
        } else {
            defaultColor = Color.blue;
        }
	}
    
    public void highlight() {
        highlight(true, new NullableColor(highlightColor));
    }

    public void unhighlight() {
        highlight(false, null);
    }

    public void highlight(Color color) {
        highlight(true, new NullableColor(color));
    }

    private void highlight(bool doHighlight, NullableColor color) {
        if (highlightMesh != null) {
            highlightMesh.SetActive(doHighlight);
            return;
        }
        if (_renderer == null) {
            return;
        }
        if (doHighlight) {
            if (color != null) {
                _renderer.material = defaultMaterial;
                _renderer.material.color = color.color;
            } else {
                _renderer.material = highlightMaterial;
            }
        } else {
            _renderer.material = defaultMaterial;
            if (defaultMaterial.HasProperty("_Color")) {
                _renderer.material.color = defaultColor;
            }
        }
    }
    protected class NullableColor
    {
        public Color color;
        public NullableColor(Color color) {
            this.color = color;
        }
    }
}

