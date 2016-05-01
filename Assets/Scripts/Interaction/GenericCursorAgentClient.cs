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

    public void disconnect() {
    }

    public void dragOverride(VectorXZ cursorGlobal) {
    }

    public void endDragOverride(VectorXZ cursorGlobal) {
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

    public void startDragOverride(VectorXZ cursorGlobal, Collider dragOverrideCollider) {
    }

    public void suspendConnection() {
    }

    public void triggerExitDuringDrag(Collider other) {
    }

    public void handleTriggerEnter(Collider other) {
        //TODO: highlight other? is this used by things that connect?
    }
}
