using UnityEngine;
using System.Collections;
using System;

public class HandleRotatorClient : MonoBehaviour , ICursorAgentClient {

    protected HandleRotator handleRotator;
    protected Collider _mainCollider;

    public void Awake() {
        handleRotator = GetComponent<HandleRotator>();
        _mainCollider = GetComponentInChildren<Collider>();
    }


    public bool connectTo(Collider other) {
        return false;
    }

    //public void disconnect() {
    //}

    public void dragOverride(VectorXZ cursorGlobal) {
        handleRotator.dragOverride(cursorGlobal);
    }

    public void endDragOverride(VectorXZ cursorGlobal) {
        handleRotator.endDragOverride(cursorGlobal);
    }

    public void handleTriggerEnter(Collider other) {

    }

    public Collider mainCollider() {
        return _mainCollider;
    }

    public bool makeConnectionWithAfterCursorOverride(Collider other) {
        return false;
    }

    public void normalDrag(VectorXZ cursorPos) {
        throw new NotImplementedException();
    }

    public void normalDragEnd(VectorXZ cursorPos) {
        throw new NotImplementedException();
    }

    public void normalDragStart(VectorXZ cursorPos) {
        throw new NotImplementedException();
    }

    public void onDragEnd() {
        handleRotator.onDragEnd();
    }

    public Collider shouldPreserveConnection() {
        return null;
    }

    public void startDragOverride(VectorXZ cursorGlobal, Collider dragOverrideCollider) {
        handleRotator.startDragOverride(cursorGlobal, dragOverrideCollider);
    }

    public void suspendConnection() {
    }

    public void triggerExitDuringDrag(Collider other) {
    }
}
