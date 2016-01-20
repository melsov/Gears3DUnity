using UnityEngine;
using System.Collections;
using System;

public class HandleRotator : MonoBehaviour , ICursorAgentClient {

    protected HandleSet handleSet;
    protected Transform _cursorRotationPivot = null;
    protected Transform _cursorRotationHandle = null;
    protected Collider _mainCollider;

    public void startDragOverride(VectorXZ cursorGlobal, Collider dragOverrideCollider) {
        vStartDragOverride(cursorGlobal, dragOverrideCollider);
    }
    public void dragOverride(VectorXZ cursorGlobal) { 
        vDragOverride(cursorGlobal);
    }
    public void endDragOverride(VectorXZ cursorGlobal) {
        vEndDragOverride(cursorGlobal);
    }

    protected virtual void updateCursorRotationPivot(Collider dragOverrideCollider) {
        if (_cursorRotationPivot == null) {
            _cursorRotationPivot = this.transform;
            HandleSet handleSet = dragOverrideCollider.GetComponentInParent<HandleSet>();
            if (handleSet == null) {
                return;
            }
            if (handleSet != null) {
                Handle other = handleSet.getAnotherThatIsntThisOne(dragOverrideCollider.GetComponent<Handle>());
                if (other != null) {
                    _cursorRotationPivot = other.transform;
                }
            }
        }
    }

    protected virtual void removeConstraintsFromWidget(Handle handle) {
        if (handle.widget == null) return;
        Socket socket = handle.widget.GetComponent<Socket>();
        if (socket != null) {
            socket.removeConstraint();
        }
    }

    protected virtual void vStartDragOverride(VectorXZ cursorGlobal, Collider dragOverrideCollider) {
        removeConstraintsFromWidget(dragOverrideCollider.GetComponent<Handle>());
        _cursorRotationHandle = dragOverrideCollider.transform;
        _cursorRotationPivot = null;
        updateCursorRotationPivot (dragOverrideCollider);
    }

    protected virtual void vDragOverride(VectorXZ cursorGlobal) {
        // rotate around the pivot
        Vector3 current = _cursorRotationHandle.position - _cursorRotationPivot.position;
        Vector3 target = cursorGlobal.vector3(_cursorRotationPivot.position.y) - _cursorRotationPivot.position;
        transform.RotateAround(_cursorRotationPivot.position, EnvironmentSettings.towardsCameraDirection, Quaternion.FromToRotation(current, target).eulerAngles.y);
    }

    protected virtual void vEndDragOverride(VectorXZ cursorGlobal) {
    }

    public Collider mainCollider() {
        return _mainCollider;
    }
    #region unused icursoragent

    public bool makeConnectionWithAfterCursorOverride(Collider other) {
        return false;
    }

    public Collider shouldPreserveConnection() {
        return null;
    }

    public void suspendConnection() {
    }

    public void triggerExitDuringDrag(Collider other) {
    }
    
    public bool connectTo(Collider other) {
        return false;
    }

    public void disconnect() {
    }
    #endregion

    // Use this for initialization
    void Awake () {
        handleSet = GetComponent<HandleSet>();
        _mainCollider = TransformUtil.FindComponentInThisOrChildren<Collider>(transform);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
