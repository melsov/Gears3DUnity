using UnityEngine;
using System.Collections;

public class RotationHandle : Handle {

    protected Transform _cursorRotationPivot = null;

    public void startRotateAround(Transform pivot) {
        _cursorRotationPivot = pivot;
    }
    public void rotateAround(VectorXZ cursorGlobal) {
        Vector3 current = transform.position - _cursorRotationPivot.position;
        Vector3 target = cursorGlobal.vector3(_cursorRotationPivot.position.y) - _cursorRotationPivot.position;
        _cursorRotationPivot.RotateAround(_cursorRotationPivot.position, EnvironmentSettings.towardsCameraDirection, Quaternion.FromToRotation(current, target).eulerAngles.y);        
    }
}
