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
        client.disconnect();
        dragOverrideCollider = null;
        dragOverrideCollider = RayCastUtil.getColliderUnderCursor(dragOverrideLayer, out rayHit);
        if(overridingDrag) {
            client.startDragOverride(cursorGlobal, dragOverrideCollider);
        }
    }

    public bool shouldOverrideDrag(VectorXZ cursorGlobal) {
        return overridingDrag;
    }

    //TODO: fix problem with drags that never leave their former drivable/parent's collider: can't reconnect to them
    //TODO: add drag overriding mechanism to CursorInteractable
    public void cursorInteracting(VectorXZ cursorGlobal) {
        if (overridingDrag) {
            client.dragOverride(cursorGlobal);
        }
    }

    public void endCursorInteraction(VectorXZ cursorGlobal) {
        _cursorInteracting = false;
        if (overridingDrag) {
            client.endDragOverride(cursorGlobal);
        } else {
            connectToColliders();
        }
    }

    public bool cursorInteracting() {
        return _cursorInteracting;
    }

    private void connectToColliders() {
        while(colliderDropper.colliders.Count > 0) {
            Collider c = colliderDropper.colliders[0];
            colliderDropper.colliders.RemoveAt(0);
            if (client.connectTo(c)) {
                colliderDropper.removeAll();
                return;
            }
        }
    }

}

public interface ICursorAgentClient
{
    void suspendConnection();
    Collider shouldPreserveConnection();
    void disconnect();
    bool connectTo(Collider other);
    void startDragOverride(VectorXZ cursorGlobal, Collider dragOverrideCollider);
    void dragOverride(VectorXZ cursorGlobal);
    void endDragOverride(VectorXZ cursorGlobal);
}
