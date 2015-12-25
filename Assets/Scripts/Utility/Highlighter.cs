﻿using UnityEngine;
using System.Collections;

public class Highlighter : MonoBehaviour {

    private Renderer _renderer;
    private Color defaultColor;
    public Color highlightColor = Color.red;

	// Use this for initialization
	void Awake () {
        _renderer = GetComponent<Renderer>();
        if (_renderer == null) {
            _renderer = GetComponentInChildren<Renderer>();
        }
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

    public void unhighlight() {
        material.color = defaultColor;
    }

}
