using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System;

public class CursorAgent : MonoBehaviour, ICursorInteractable, IColliderDropperClient {

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
        Assert.IsTrue(client != null, "null cursor agent client: " + name);
        dragOverrideLayer = LayerMask.GetMask("DragOverride");
    }

    //TODO: pegs on top of handles are hard to grab/remove from their parent sockets: give them priority over handles when selecting
    //TODO: let drivable handle move itself! move code out of CursorInteraction: will need to add / rearrange some ICursorAgentClient methods
    //Impetus for this: LinearActuator needs to test whether it should disconnect from its constraint pair thing as it moves (or at least at the end of the move)
    //TODO: BUG: disconnecting pole sometimes causes it to fly away violently or glide gently. Ensure that velocity is zero when disconnecting?
    public void startCursorInteraction(VectorXZ cursorGlobal) {
        _cursorInteracting = true;
        _dragOverrideCollider = null;
        _dragOverrideCollider = RayCastUtil.getColliderUnderCursor(dragOverrideLayer, out rayHit);
        if (client != null) {
            if (overridingDrag) {
                client.startDragOverride(cursorGlobal, _dragOverrideCollider);
            } else {
                //client.disconnect();
                client.normalDragStart(cursorGlobal);
                disableCollider(true);
            }
        }
    }

    private void disableCollider(bool disable) {
        client.mainCollider().enabled = !disable;
    }

    public bool shouldOverrideDrag(VectorXZ cursorGlobal) {
        return overridingDrag;
    }

    public void cursorInteracting(VectorXZ cursorGlobal) {
        if (client == null) { return; }
        disableCollider(false);
        if (overridingDrag) {
            client.dragOverride(cursorGlobal);
        } else {
            client.normalDrag(cursorGlobal);
        }
    }
    
    public void handleTriggerEnter(Collider other) {
        client.handleTriggerEnter(other);
    }
    public void handleTriggerExit(Collider other) {
        if (client == null) { return; }
        client.triggerExitDuringDrag(other);
    }

    public void endCursorInteraction(VectorXZ cursorGlobal) {
        _cursorInteracting = false;
        unhighlight();
        if (client == null) { return; }
        if (overridingDrag) {
            client.endDragOverride(cursorGlobal);
        } else {
            client.normalDragEnd(cursorGlobal);
        }
        connectToColliders(colliderDropper);
        client.onDragEnd();
    }

    private void unhighlight() {
        if (colliderDropper == null || colliderDropper.colliders == null) { return; }
        foreach(Collider c in colliderDropper.colliders) {
            Cog cog = c.GetComponentInParent<Cog>();
            if (cog == null) { continue; }
            Highlighter h = cog.GetComponentInChildren<Highlighter>();
            if (h == null) { continue; }
            h.unhighlight();
        }
    }

    public bool isCursorInteracting() {
        return _cursorInteracting;
    }

    private void connectToColliders(ColliderDropper dropper) {
        if (dropper == null) {
            return;
        }
        while(dropper.colliders.Count > 0) {
            Collider c = dropper.colliders[0];
            dropper.colliders.RemoveAt(0);
            unhighlight(c);
            bool done = !overridingDrag ? client.connectTo(c) : client.makeConnectionWithAfterCursorOverride(c);
            if (done) { 
                break;
            }
        }
        if (client is ICursorAgentClientExtended) {
            while (dropper.escapedFromColliders.Count > 0) {
                Collider c = dropper.escapedFromColliders[0];
                dropper.escapedFromColliders.RemoveAt(0);
                ((ICursorAgentClientExtended)client).handleEscapedFromCollider(c);
            }
        }
        dropper.removeAll();
    }

    // CONSIDER: the need for this function shows problems with the collider dropper / cursor agent system 
    private void unhighlight(Collider c) {
        Highlighter h = c.GetComponent<Highlighter>();
        if (h != null) {
            h.unhighlight();
        }
    }

}

public interface ICursorAgentUrClient
{
    //void disconnect();
    bool connectTo(Collider other);

    void normalDragStart(VectorXZ cursorPos);
    void normalDrag(VectorXZ cursorPos);
    void normalDragEnd(VectorXZ cursorPos);

}

public interface ICursorAgentClient : ICursorAgentUrClient
{
    void handleTriggerEnter(Collider other);
    void suspendConnection();
    Collider shouldPreserveConnection();
    void onDragEnd();
    bool makeConnectionWithAfterCursorOverride(Collider other);
    void startDragOverride(VectorXZ cursorGlobal, Collider dragOverrideCollider);
    void dragOverride(VectorXZ cursorGlobal);
    void endDragOverride(VectorXZ cursorGlobal);
    Collider mainCollider();
    void triggerExitDuringDrag(Collider other);
}

public interface ICursorAgentClientExtended : ICursorAgentClient
{
    void handleEscapedFromCollider(Collider other);
}
