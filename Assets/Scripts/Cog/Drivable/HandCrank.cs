using UnityEngine;
using System.Collections;

public class HandCrank : Motor {

    protected override void awake() {
        base.awake();
        handleSet.handles[0].widget = axel.transform;
    }

    protected Transform widget {
        get { return handleSet.handles[0].widget; }
    }

    protected override void vDragOverride(VectorXZ cursorGlobal) {
        // rotate around the pivot
        Vector3 current = _cursorRotationHandle.position - _cursorRotationPivot.position;
        Vector3 target = cursorGlobal.vector3(_cursorRotationPivot.position.y) - _cursorRotationPivot.position;
        handleSet.transform.RotateAround(_cursorRotationPivot.position, EnvironmentSettings.towardsCameraDirection, Quaternion.FromToRotation(current, target).eulerAngles.y);
        angle = handleSet.transform.rotation.eulerAngles.y;
    }

    protected override void update() {
        axel.turnTo(angle);
    }
}
