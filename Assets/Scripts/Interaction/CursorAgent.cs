using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System;

public class CursorAgent : MonoBehaviour, CursorInteractable, IColliderDropperClient {

    private ColliderDropper colliderDropper;
    private bool _cursorInteracting;
    public ICursorAgentClient client;

    private Collider preservedCollider;
    private int dragOverrideLayer;
    private RaycastHit rayHit;
    
    private bool overridingDrag {
        get { return _dragOverrideCollider != null; }
    }
    private Collider _dragOverrideCollider;
    public Collider dragOverrideCollider {
        get { return _dragOverrideCollider; }
    }

    void Awake() {
        colliderDropper = GetComponent<ColliderDropper>();
        if (colliderDropper == null) {
            colliderDropper = GetComponentInChildren<ColliderDropper>();
        }
        client = GetComponent<ICursorAgentClient>();
        Assert.IsTrue(client != null);
        dragOverrideLayer = LayerMask.GetMask("DragOverride");
    }

    //TODO: pegs on top of handles are hard to grab/remove from their parent sockets: give them priority over handles when selecting
    public void startCursorInteraction(VectorXZ cursorGlobal) {
        _cursorInteracting = true;
        _dragOverrideCollider = null;
        _dragOverrideCollider = RayCastUtil.getColliderUnderCursor(dragOverrideLayer, out rayHit);
        if (overridingDrag) {
            client.startDragOverride(cursorGlobal, _dragOverrideCollider);
        } else {
            client.disconnect();
            disableCollider(true);
        }
    }

    private void disableCollider(bool disable) {
        client.mainCollider().enabled = !disable;
    }

    public bool shouldOverrideDrag(VectorXZ cursorGlobal) {
        return overridingDrag;
    }

    //TODO: fix problem with drags that never leave their former drivable/parent's collider: can't reconnect to them
    //TODO: add drag overriding mechanism to CursorInteractable
    public void cursorInteracting(VectorXZ cursorGlobal) {
        disableCollider(false);
        if (overridingDrag) {
            client.dragOverride(cursorGlobal);
        }
    }

    public void handleTriggerExit(Collider other) {
        client.triggerExitDuringDrag(other);
    }

    public void endCursorInteraction(VectorXZ cursorGlobal) {
        _cursorInteracting = false;
        if (overridingDrag) {
            client.endDragOverride(cursorGlobal);
            //if (handle != null && handle.GetComponent<ColliderDropper>() != null) {
            //    print("got component collider dropper from handle");
            //    connectToColliders(handle.GetComponent<ColliderDropper>());
            //    Destroy(handle.GetComponent<ColliderDropper>());
            //    return;
            //}
        } 
        connectToColliders(colliderDropper);
    }

    public bool isCursorInteracting() {
        return _cursorInteracting;
    }

    private void connectToColliders(ColliderDropper dropper) {
        while(dropper.colliders.Count > 0) {
            Collider c = dropper.colliders[0];
            dropper.colliders.RemoveAt(0);
            unhighlight(c);
            bool done = !overridingDrag ? client.connectTo(c) : client.makeConnectionWithAfterCursorOverride(c);
            if (done) { 
                dropper.removeAll();
                return;
            }
        }
    }

    // CONSIDER: the need for this function shows problems with the collider dropper / cursor agent system : for now: 'oh well'
    private void unhighlight(Collider c) {
        Highlighter h = c.GetComponent<Highlighter>();
        if (h != null) {
            h.unhighlight();
        }
    }

}

public interface ICursorAgentClient
{
    void suspendConnection();
    Collider shouldPreserveConnection();
    void disconnect();
    bool connectTo(Collider other);
    bool makeConnectionWithAfterCursorOverride(Collider other);
    void startDragOverride(VectorXZ cursorGlobal, Collider dragOverrideCollider);
    void dragOverride(VectorXZ cursorGlobal);
    void endDragOverride(VectorXZ cursorGlobal);
    Collider mainCollider();
    void triggerExitDuringDrag(Collider other);
}
