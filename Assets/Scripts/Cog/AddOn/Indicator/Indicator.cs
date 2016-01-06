﻿using UnityEngine;
using System.Collections;

public class Indicator : MonoBehaviour {

    protected Renderer _renderer;
    public Color onColor = Color.green;
    public Color offColor = Color.gray;
    public Color reverseColor = Color.red;

    protected AddOn addOn;

	void Awake () {
        awake();
	}
    protected virtual void awake() {
        _renderer = GetComponent<Renderer>();
        addOn = GetComponentInParent<AddOn>(); // TransformUtil.FindTypeInParentRecursive<AddOn>(transform, 3);
    }

    protected virtual void updateIndicator() { }
}