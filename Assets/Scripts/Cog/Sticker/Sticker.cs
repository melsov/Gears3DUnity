using UnityEngine;
using System.Collections;
using System;

//TODO: sticker appears always over gears and all

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

    protected Transform unscaledParent(Transform t) {
        while (!t.localScale.Equals(new Vector3(1,1,1))) {
            if (t.parent == null) { return t; }
            t = t.parent;
        }
        return t;
    }

// ???
//    public bool connectTo(Collider other) {
//        Cog cog = other.GetComponentInParent<Cog>();
//        if (cog == null) { print("no cog in sticker"); return false; }
//        Transform target = unscaledParent(other.transform);
//        transform.parent = target; // cog.transform;
//// TODO: ensure that camera isn't below the bow collider
//        transform.position = TransformUtil.SetY(transform.position, Mathf.Max(YLayer.Layer(typeof(Sticker)), target.transform.position.y + 1f));
//        return true;
//    }

    //public void disconnect() {
    //    transform.parent = null;
    //    foreach(Sticker s in GetComponentsInChildren<Sticker>()) {
    //        if (s == this) { continue; }
    //        s.disconnect();
    //    }
    //}
    
    public void startDragOverride(VectorXZ cursorGlobal, Collider dragOverrideCollider) {
        //disconnect(); //pepperonis
        handleRotator.startDragOverride(cursorGlobal, dragOverrideCollider);
    }

    public void dragOverride(VectorXZ cursorGlobal) {
        handleRotator.dragOverride(cursorGlobal);
    }

    public void endDragOverride(VectorXZ cursorGlobal) {
        handleRotator.endDragOverride(cursorGlobal);
    }

    public void triggerExitDuringDrag(Collider other) {
        unhighlight(other.transform);
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

    public void handleTriggerEnter(Collider other) {
        highlight(other.transform);

    }

    public override ProducerActions producerActionsFor(Cog client, ContractSpecification specification) {
        throw new NotImplementedException();
    }

    public override ClientActions clientActionsFor(Cog producer, ContractSpecification specification) {
        throw new NotImplementedException();
    }

    protected override ContractSiteBoss getConnectionSiteBoss() {
        throw new NotImplementedException();
    }

    public override ConnectionSiteAgreement.ConnektAction connektActionAsTravellerFor(ContractSpecification specification) {
        throw new NotImplementedException();
    }
}
