using UnityEngine;
using System.Collections;
using System;

public class GenericCursorAgentClient : MonoBehaviour , ICursorAgentClient {

    protected Collider _collider;

    void Awake() {
        _collider = GetComponentInChildren<Collider>();
    }

    public bool connectTo(Collider other) {
        return false;
    }


    public virtual bool wouldConnectTo(Collider collider) {
        return false;
    }

    //public void disconnect() {
    //}
    public void startDragOverride(CursorInfo ci) { //VectorXZ cursorGlobal, Collider dragOverrideCollider) {
    }

    public void dragOverride(CursorInfo ci) { // VectorXZ cursorGlobal) {
    }

    public void endDragOverride(CursorInfo ci) { //VectorXZ cursorGlobal) {
    }

    public Collider mainCollider() {
        return _collider;
    }

    public bool makeConnectionWithAfterCursorOverride(Collider other) {
        return false;
    }

    public void onDragEnd() {
    }

    public Collider shouldPreserveConnection() {
        return null;
    }


    public void suspendConnection() {
    }

    public void triggerExitDuringDrag(Collider other) {
    }

    public void handleTriggerEnter(Collider other) {
        //TODO: highlight other? is this used by things that connect?
    }

    public void normalDragStart(VectorXZ cursorPos) {
        throw new NotImplementedException();
    }

    public void normalDrag(VectorXZ cursorPos) {
        throw new NotImplementedException();
    }

    public void normalDragEnd(VectorXZ cursorPos) {
        throw new NotImplementedException();
    }

    public void handleEscapedFromCollider(Collider other) {
        throw new NotImplementedException();
    }
}
