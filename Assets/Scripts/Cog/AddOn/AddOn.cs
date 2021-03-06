﻿using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class AddOn : Cog , ICursorAgentClient
{
    //public bool shouldPositionOnConnect = true;
    public bool shouldFollowClient = false; //??? WANT (hopefully we don't anymore)??? true;
    //public IAddOnClient client; //TODO: purge client from add on? or make into a property at least...don't use where unneeded
    //public bool hasClient {
    //    get { return client != null; }
    //}

    //public bool isClient(IAddOnClient aoc) { return hasClient && aoc == client; }

    protected Collider currentOverrideCollider;
    protected RotationHandle rotationHandle;

    #region ICursorAgentClient
    public void handleTriggerEnter(Collider other) {

    }
    //public bool connectTo(Collider other) { return vConnectTo(other); }

/* rest in peace
    protected bool vConnectTo(Collider other) {
        Cog cog = other.GetComponentInParent<Cog>();
        if (connectToClient(cog)) {
            return true;
        }
        return false;
    }
*/

    protected Follower _follower;
    protected Follower follower {
        get {
            if (_follower == null) {
                _follower = gameObject.AddComponent<Follower>();
            }
            return _follower;
        }
    }


    public virtual bool connectToClient(Cog cog) {
        IAddOnClient aoc = cog.GetComponentInParent<IAddOnClient>();
        if (aoc != null) {
            if (aoc.connectToAddOn(this)) {
                if (!TransformUtil.IsDescendent(transform, cog.transform)) {
                    cog.positionRelativeToAddOn(this);
                    if (shouldFollowClient) {
                        follower.offset = transform.position - cog.transform.position;
                        follower.target = cog.transform;
                    }
                }
                //client = aoc;
                return true;
            }
        }
        return false;
    }


    protected virtual void positionOnConnect(Cog cog) {
        Vector3 pos = transform.position;
        pos.x = cog.transform.position.x;
        Collider other = cog.GetComponentInChildren<Collider>();
        pos.z = other != null ? Vector3.Lerp(other.bounds.center, other.bounds.max, .85f).z : pos.z;
        transform.position = pos;
    }

   // public void disconnect() { vDisconnect(); }

/* pepperoni
    protected virtual void vDisconnect() {
        Debug.LogError("vDisconn in AddOn: client null? " + (client == null));
        if (client != null) {
            client.forgetAbout(this);
            //client.disconnectAddOn(this);
            if (_follower != null) {
                _follower.target = null;
            }
            client = null;
        }
    }
*/
    public void startDragOverride(CursorInfo ci) { vStartDragOverride(ci); }
    public void dragOverride(CursorInfo ci) { vDragOverride(ci); }
    public void endDragOverride(CursorInfo ci) { vEndDragOverride(ci); }

    private void updateRotationHandle(Collider dragOverrideCollider) {
        rotationHandle = null;
        if (dragOverrideCollider.GetComponent<RotationHandle>() != null) {
            rotationHandle = dragOverrideCollider.GetComponent<RotationHandle>();
        }
    }

    protected virtual void vStartDragOverride(CursorInfo ci) { //VectorXZ cursorGlobal, Collider dragOverrideCollider) {
            updateRotationHandle(ci.collider); //dragOverrideCollider);
        if (rotationHandle != null) {
            rotationHandle.startRotateAround(transform);
        }
    }
    protected virtual void vDragOverride(CursorInfo ci) { // VectorXZ cursorGlobal) {
        if (rotationHandle != null) {
            rotationHandle.rotateAround(ci.current);
        }
    }
    protected virtual void vEndDragOverride(CursorInfo ci) {//  VectorXZ cursorGlobal) {
        
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

    protected override void awake() {
        base.awake();
        rotationHandle = GetComponentInChildren<RotationHandle>();
        _follower = GetComponent<Follower>();
    }
	
	// Update is called once per frame
	void Update () {
        update();
	}
    protected virtual void update() {
        
    }

    public void onDragEnd() {
    }

    
}

public interface IAddOnClient
{
    bool connectToAddOn(AddOn addOn_);
    void forgetAbout(AddOn addOn_);
}
