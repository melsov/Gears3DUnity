using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System;

//CONSIDER: instead of all the 'isClient' conditions, make a sub-class for CursorAgents that deal with full fledged clients
public class CursorAgent : MonoBehaviour, ICursorInteractable, IColliderDropperClient {

    private ColliderDropper colliderDropper;
    private bool _cursorInteracting;
    public ICursorAgentUrClient urClient;
    private ICursorAgentClient client {
        get {
            if (isClient) { return (ICursorAgentClient)urClient; }
            return null;
        }
    }
    private bool isClient { get { return urClient is ICursorAgentClient; } }

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

    private CursorInfo cursorInfo = CursorInfo.FakeNull();

    void Awake() {
        colliderDropper = GetComponent<ColliderDropper>();
        if (colliderDropper == null) {
            colliderDropper = GetComponentInChildren<ColliderDropper>();
        }
        urClient = GetComponent<ICursorAgentUrClient>();

        Assert.IsTrue(urClient != null, "null cursor agent Ur client: " + name);
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
        cursorInfo = new CursorInfo(cursorGlobal, cursorGlobal, _dragOverrideCollider);
        if (urClient != null) {
            if (overridingDrag) {
                if (isClient)
                    client.startDragOverride(cursorInfo); // cursorGlobal, _dragOverrideCollider);
            } else {
                //client.disconnect();
                urClient.normalDragStart(cursorGlobal);
                disableCollider(true);
            }
        }
    }

    private void disableCollider(bool disable) {
        if (isClient)
            client.mainCollider().enabled = !disable;
    }

    public bool shouldOverrideDrag(VectorXZ cursorGlobal) {
        return overridingDrag;
    }

    public void cursorInteracting(VectorXZ cursorGlobal) {
        cursorInfo.current = cursorGlobal;
        if (urClient == null) { return; }
        disableCollider(false);
        if (overridingDrag) {
            if (isClient) client.dragOverride(cursorInfo); // cursorGlobal);
        } else {
            checkWouldConnect();
            urClient.normalDrag(cursorGlobal);
        }
    }

    private void checkWouldConnect() {
        if (!colliderDropper) { return; }
        foreach(Collider c in colliderDropper.colliders) {
            if (urClient.wouldConnectTo(c)) { return; }
        }
    }

    public void handleTriggerEnter(Collider other) {
        if (isClient) client.handleTriggerEnter(other);
    }
    public void handleTriggerExit(Collider other) {
        if (urClient == null) { return; }
        //if (isClient)
            client.triggerExitDuringDrag(other);
    }

    public void endCursorInteraction(VectorXZ cursorGlobal) {
        cursorInfo.current = cursorGlobal;
        _cursorInteracting = false;
        unhighlight();
        if (urClient == null) { return; }
        if (overridingDrag) {
            if (isClient) client.endDragOverride(cursorInfo); // cursorGlobal);
        } else {
            urClient.normalDragEnd(cursorGlobal);
        }
        connectToColliders(colliderDropper);
        if (isClient) client.onDragEnd();
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
            bool done;
            if (isClient && overridingDrag) {
                done = client.makeConnectionWithAfterCursorOverride(c);
            } else {
                done = urClient.connectTo(c);
            }
            if (done) { 
                break;
            }
        }
        
        while (dropper.escapedFromColliders.Count > 0) {
            Collider c = dropper.escapedFromColliders[0];
            dropper.escapedFromColliders.RemoveAt(0);
            urClient.handleEscapedFromCollider(c);
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
    bool wouldConnectTo(Collider other);
    void normalDragStart(VectorXZ cursorPos);
    void normalDrag(VectorXZ cursorPos);
    void normalDragEnd(VectorXZ cursorPos);
    void triggerExitDuringDrag(Collider other);

    void handleEscapedFromCollider(Collider other);

}

public struct CursorInfo
{
    public readonly VectorXZ start;
    public VectorXZ current;
    public readonly Collider collider;

    public VectorXZ relativeToStart { get { return current - start; } }
    
    public CursorInfo(VectorXZ start, VectorXZ current, Collider collider) {
        this.start = start;
        this.current = current;
        this.collider = collider;
    }

    public static CursorInfo FakeNull() {
        return new CursorInfo(VectorXZ.fakeNull, VectorXZ.fakeNull, null);
    }

    public bool isFakeNull() { return start.isFakeNull(); }

    public static implicit operator VectorXZ(CursorInfo ci) { return ci.current; }
}

public interface ICursorAgentClient : ICursorAgentUrClient
{
    void handleTriggerEnter(Collider other);
    void suspendConnection();
    Collider shouldPreserveConnection();
    void onDragEnd();
    bool makeConnectionWithAfterCursorOverride(Collider other);
    void startDragOverride(CursorInfo cursorInfo); // VectorXZ cursorGlobal, Collider dragOverrideCollider);
    void dragOverride(CursorInfo cursorInfo); //  VectorXZ cursorGlobal);
    void endDragOverride(CursorInfo cursorInfo); // VectorXZ cursorGlobal);
    Collider mainCollider();
    //void triggerExitDuringDrag(Collider other);
}

public interface ICursorAgentClientExtended : ICursorAgentClient
{
    //void handleEscapedFromCollider(Collider other);
}
