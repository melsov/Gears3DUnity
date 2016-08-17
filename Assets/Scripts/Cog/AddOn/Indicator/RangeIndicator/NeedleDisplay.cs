using UnityEngine;
using System.Collections;
using System;

public class NeedleDisplay : RangeIndicator
{
    [SerializeField]
    protected Transform needle;
    protected Arc arc;

    public float max = 50f;

    public override void Awake() {
        base.Awake();
        arc = GetComponentInChildren<Arc>();
    }

    protected override void updateDisplay(float f) {
        Quaternion q = arc.between(Mathf.Clamp01(f / max));
        needle.rotation = q;
    }
}
