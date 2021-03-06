﻿using UnityEngine;
using System.Collections;
using System;

public class ValueDisplay : RangeIndicator {

    protected TextMesh textMesh;
    
    public override void Awake() {
        base.Awake();
        textMesh = GetComponentInChildren<TextMesh>();
    }

    protected override void updateDisplay(float f) {
        textMesh.text = string.Format("{0}", f.ToString("n1"));
    }
}
