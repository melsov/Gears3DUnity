using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Highlighter : MonoBehaviour {

    //public Renderer _renderer;
    //private Color defaultColor;
    //public Color highlightColor = Color.red;
    //private GameObject highlightMesh;
    //private string ColorProperty = "_Color";
    private Highlightable[] highlightables;
    public Color defaultColor = Color.red;

    private void setupHighlightables() {
        List<Highlightable> result = new List<Highlightable>();
        
        foreach(Renderer r in GetComponentsInChildren<Renderer>()) {// manifest.componentsOfType<Renderer>()) {
            print("hi");
            Highlightable h = r.GetComponent<Highlightable>();
            if (h == null) {
                h = r.gameObject.AddComponent<Highlightable>();
            }
            result.Add(h);
        }
        highlightables = result.ToArray();
    }

	void Awake () {
        setupHighlightables();
        //foreach (Transform t in GetComponentInChildren<Transform>()) {
        //    if (t.CompareTag("HighlightMesh")) {
        //        highlightMesh = t.gameObject;
        //        highlightMesh.SetActive(false);
        //        break;
        //    }
        //}
        //if (_renderer == null) {
        //    _renderer = GetComponent<Renderer>();
        //}
        //if (_renderer == null) {
        //    _renderer = GetComponentInChildren<Renderer>();
        //}
        //Bug.assertPause(_renderer.materials[0] != null, "_renderer [0] is null?");
        //Color cTest = material.GetColor(ColorProperty);
        //print("color is: " + cTest.ToString());
        //if (material.GetColor(ColorProperty) != null) { defaultColor = material.GetColor(ColorProperty); } 
        //else { defaultColor = material.color != null ? material.color : Color.white; }
	}

    //private Material material {
    //    get {
    //        return _renderer.materials[0];
    //    }
    //}
	
    public void highlight() {
        highlight(defaultColor, true);
        //highlight(highlightColor);
    }

    //public void highlight(Color color) {
    //    if (highlightMesh != null) {
    //        highlightMesh.SetActive(true);
    //    } else {
    //        if (material.GetColor(ColorProperty) != null) {
    //            material.SetColor(ColorProperty, color);
    //        } else {
    //            material.color = color;
    //        }
    //    }
    //}
    private void highlight(Color color, bool doHighlight) {
        foreach(Highlightable h in highlightables) {
            if (doHighlight) {
                h.highlight(color);
            } else {
                h.unhighlight();
            }
        }
    }

    //public void highlightForSeconds(float seconds) {
    //    highlightForSeconds(seconds, highlightColor);
    //}
    public void highlightForSeconds(float seconds, Color color) {
        StartCoroutine(_highlightForSeconds(seconds, color));
    }
    protected IEnumerator _highlightForSeconds(float seconds, Color color) {
        highlight(color, true);
        yield return new WaitForSeconds(seconds);
        unhighlight();
    }

    public void unhighlight() {
        highlight(defaultColor, false);
        //if (highlightMesh != null) {
        //    highlightMesh.SetActive(false);
        //} else {
        //    if (material.GetColor(ColorProperty) != null) {
        //        material.SetColor(ColorProperty, defaultColor);
        //    } else {
        //        material.color = defaultColor;
        //    }
        //}
    }

}
