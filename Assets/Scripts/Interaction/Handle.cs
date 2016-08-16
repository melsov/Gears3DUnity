using UnityEngine;
using System.Collections;
using System;

public class Handle : MonoBehaviour  {

    public Transform widget;
    [SerializeField]
    protected Axis motionAxis = Axis.Z;
    private AxisUtil3 _axisUtil;
    private AxisUtil3 axisUtil {
        get {
            if(_axisUtil == null) {
                _axisUtil = new AxisUtil3(motionAxis);
            }
            return _axisUtil;
        }
    }

    public void Awake() {
        awake();
    }

    protected virtual void awake() {
        gameObject.layer = LayerLookup.DragOverride;
    }

    public void positionOnAxis(float axisPos) {
        transform.position = axisUtil.positionOnAxis(transform.position, axisPos);
    }

    public float axisPosition {
        get { return axisUtil.axisPosition(transform.position); }
    }

}
