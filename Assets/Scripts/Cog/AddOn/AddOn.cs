using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class AddOn : MonoBehaviour , ICursorAgentClient {

    protected IAddOnClient client;

    #region ICursorAgentClient
    public bool connectTo(Collider other) { return vConnectTo(other); }
    protected virtual bool vConnectTo(Collider other) {
        print("add on v Connect to");
        IAddOnClient aoc = other.GetComponent<IAddOnClient>();
        if (aoc != null) {
            if (aoc.connectToAddOn(this)) {
                client = aoc;
                print("got a client in add on");
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

    protected virtual void vStartDragOverride(VectorXZ cursorGlobal, Collider dragOverrideCollider) {
        
    }
    protected virtual void vDragOverride(VectorXZ cursorGlobal) {
        
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
