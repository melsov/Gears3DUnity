using UnityEngine;
using System.Collections;

public class Highlightable : MonoBehaviour {
    [SerializeField]
    protected Renderer _renderer;
    protected Material defaultMaterial;
    [SerializeField]
    protected Material highlightMaterial;
    private GameObject highlightMesh;
    public Color highlightColor = Color.red;

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
	}
    
    public void highlight() {
        highlight(true, highlightColor);
    }

    public void unhighlight() {
        highlight(false, highlightColor);
    }

    public void highlight(Color color) {
        highlight(true, color);
    }

    private void highlight(bool doHighlight, Color color) {
        if (highlightMesh != null) {
            highlightMesh.SetActive(doHighlight);
            return;
        }
        if (doHighlight) {
            highlightMaterial.color = highlightColor;
        }
        _renderer.material = doHighlight ? highlightMaterial : defaultMaterial;
    }
}
