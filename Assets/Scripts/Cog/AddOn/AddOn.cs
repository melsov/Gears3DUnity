using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class AddOn : MonoBehaviour , ICursorAgentClient {

    protected IAddOnClient client;

    protected Collider currentOverrideCollider;
    protected RotationHandle rotationHandle;

    #region ICursorAgentClient
    public bool connectTo(Collider other) { return vConnectTo(other); }

    protected virtual bool vConnectTo(Collider other) {
        IAddOnClient aoc = other.GetComponent<IAddOnClient>();
        if (aoc != null) {
            if (aoc.connectToAddOn(this)) {
                client = aoc;
                return true;
            }
        }
        return false;
    }

    public void disconnect() { vDisconnect(); }
    protected virtual void vDisconnect() {
        if (client != null) {
            client.disconnectAddOn(this);
            client = null;
        }
    }

    public void startDragOverride(VectorXZ cursorGlobal, Collider dragOverrideCollider) { vStartDragOverride(cursorGlobal, dragOverrideCollider); }
    public void dragOverride(VectorXZ cursorGlobal) { vDragOverride(cursorGlobal); }
    public void endDragOverride(VectorXZ cursorGlobal) { vEndDragOverride(cursorGlobal); }

    private void updateRotationHandle(Collider dragOverrideCollider) {
        rotationHandle = null;
        if (dragOverrideCollider.GetComponent<RotationHandle>() != null) {
            rotationHandle = dragOverrideCollider.GetComponent<RotationHandle>();
        }
    }

    protected virtual void vStartDragOverride(VectorXZ cursorGlobal, Collider dragOverrideCollider) {
        updateRotationHandle(dragOverrideCollider);
        if (rotationHandle != null) {
            rotationHandle.startRotateAround(transform);
        }
    }
    protected virtual void vDragOverride(VectorXZ cursorGlobal) {
        if (rotationHandle != null) {
            rotationHandle.rotateAround(cursorGlobal);
        }
    }
    protected virtual void vEndDragOverride(VectorXZ cursorGlobal) {
        
    }

    public Collider mainCollider() {
        Collider c = GetComponent<Collider>();
        if (c == null) {
            List<Transform> mainColliders = TagLookup.ChildrenWithTag(gameObject, TagLookup.MainCollider);
            UnityEngine.Assertions.Assert.IsTrue(mainColliders.Count < 2);
            if (mainColliders.Count == 1) {
                c = mainColliders[0].GetComponent<Collider>();
            } else {
                c = GetComponentInChildren<Collider>();
            }
        }
        return c;
    }

    public bool makeConnectionWithAfterCursorOverride(Collider other) { return vMakeConnectionWithAfterCursorOverride(other); }
    protected virtual bool vMakeConnectionWithAfterCursorOverride(Collider other) {
        return false;
    }

    public Collider shouldPreserveConnection() { return null;  }
    public void suspendConnection() {  }

    public void triggerExitDuringDrag(Collider other) {

    }
    #endregion

    void Awake () {
        awake();
	}
    protected virtual void awake() {
        rotationHandle = GetComponentInChildren<RotationHandle>();
    }
	
	// Update is called once per frame
	void Update () {
        update();
	}
    protected virtual void update() {
        
    }
}

public interface IAddOnClient
{
    bool connectToAddOn(AddOn addOn_);
    void disconnectAddOn(AddOn addOn_);
}
