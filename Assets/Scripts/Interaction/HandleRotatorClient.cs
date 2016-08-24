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

    public virtual bool wouldConnectTo(Collider collider) {
        return false;
    }
    //public void disconnect() {
    //}

    public void dragOverride(CursorInfo ci) { //VectorXZ cursorGlobal) {
        handleRotator.dragOverride(ci.current); // cursorGlobal);
    }

    public void endDragOverride(CursorInfo ci) { //VectorXZ cursorGlobal) {
        handleRotator.endDragOverride(ci.current);// cursorGlobal);
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

    public void startDragOverride(CursorInfo ci) { //VectorXZ cursorGlobal, Collider dragOverrideCollider) {
        handleRotator.startDragOverride(ci.current, ci.collider); // cursorGlobal, dragOverrideCollider);
    }

    public void suspendConnection() {
    }

    public void triggerExitDuringDrag(Collider other) {
    }

    public void handleEscapedFromCollider(Collider other) {
        throw new NotImplementedException();
    }
}
