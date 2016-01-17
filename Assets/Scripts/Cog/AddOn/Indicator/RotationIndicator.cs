using UnityEngine;
using System.Collections;

public class RotationIndicator : Indicator {

    protected float _rotation;
    public float rotation {
        set {
            _rotation = value;
            updateIndicator();
        }
    }

    protected override void updateIndicator() {
        transform.eulerAngles = new Vector3(0f, _rotation, 0f);
    }
}
