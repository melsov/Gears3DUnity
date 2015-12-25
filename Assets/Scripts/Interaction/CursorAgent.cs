using UnityEngine;
using System.Collections;
using System;

public class CursorAgent : MonoBehaviour, CursorInteractable, ColliderDropperClient {

    private ColliderDropper colliderDropper;
    private bool _cursorInteracting;
    public ICursorAgentClient client;

    private Collider preservedCollider;
    private int dragOverrideLayer;
    private RaycastHit rayHit;
    private bool overridingDrag {
        get { return dragOverrideCollider != null; }
    }
    private Collider dragOverrideCollider;

    void Awake() {
        colliderDropper = GetComponent<ColliderDropper>();
        client = GetComponent<ICursorAgentClient>();
        dragOverrideLayer = LayerMask.GetMask("DragOverride");
    }

    public void startCursorInteraction(VectorXZ cursorGlobal) {
        _cursorInteracting = true;
        dragOverrideCollider = null;
        dragOverrideCollider = RayCastUtil.getColliderUnderCursor(dragOverrideLayer, out rayHit);
        if (overridingDrag) {
            client.startDragOverride(cursorGlobal, dragOverrideCollider);
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

    public void endCursorInteraction(VectorXZ cursorGlobal) {
        _cursorInteracting = false;
        if (overridingDrag) {
            client.endDragOverride(cursorGlobal);
        } else {
        }
        connectToColliders();
    }

    public bool cursorInteracting() {
        return _cursorInteracting;
    }

    private void connectToColliders() {
        while(colliderDropper.colliders.Count > 0) {
            Collider c = colliderDropper.colliders[0];
            colliderDropper.colliders.RemoveAt(0);
            unhighlight(c);
            bool done = !overridingDrag ? client.connectTo(c) : client.makeConnectionWithAfterCursorOverride(c);
            if (done) { 
                colliderDropper.removeAll();
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
}
