﻿using UnityEngine;
using System.Collections;
using System;

public class Sticker : Cog, ICursorAgentClient
{
    protected HandleRotator handleRotator;
    protected Collider _mainCollider;
    
    void Awake() { awake(); }

    protected void awake() {
        handleRotator = GetComponent<HandleRotator>();
        _mainCollider = GetComponentInChildren<Collider>();
        transform.position = TransformUtil.SetY(transform.position, YLayer.Layer(typeof(Sticker)));
        BoxCollider bc = GetComponent<BoxCollider>();
        bc.size = TransformUtil.SetY(bc.size, transform.position.y * 2f + 1f);
    }

    public bool connectTo(Collider other) {
        Cog cog = other.GetComponentInParent<Cog>();
        if (cog == null) { return false; }
        transform.parent = other.transform;
        return true;
    }

    public void disconnect() {
        transform.parent = null;
    }
    
    public void startDragOverride(VectorXZ cursorGlobal, Collider dragOverrideCollider) {
        handleRotator.startDragOverride(cursorGlobal, dragOverrideCollider);
    }

    public void dragOverride(VectorXZ cursorGlobal) {
        handleRotator.dragOverride(cursorGlobal);
    }

    public void endDragOverride(VectorXZ cursorGlobal) {
        handleRotator.endDragOverride(cursorGlobal);
    }

    public void triggerExitDuringDrag(Collider other) {
        
    }

    public Collider mainCollider() {
        return _mainCollider;
    }

    public bool makeConnectionWithAfterCursorOverride(Collider other) {
        if (transform.parent == null) {
            return connectTo(other);
        }
        return false;
    }

    public void onDragEnd() {
    }

    public Collider shouldPreserveConnection() {
        return null;
    }

    public void suspendConnection() {
    }

}
