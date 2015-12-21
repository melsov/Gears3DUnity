using UnityEngine;
using System.Collections;
using System;

public class CursorAgent : MonoBehaviour, CursorInteractable, ColliderDropperClient {

    private ColliderDropper colliderDropper;
    private bool _cursorInteracting;
    public ICursorAgentClient client;

    private Collider preservedCollider;

    void Awake() {
        colliderDropper = GetComponent<ColliderDropper>();
        client = GetComponent<ICursorAgentClient>();
    }

    public void startCursorInteraction(VectorXZ cursorGlobal) {
        _cursorInteracting = true;
        client.disconnect();
        //preservedCollider = client.shouldPreserveConnection();
        //client.suspendConnection();
    }
    //TODO: fix drags that never leave their former drivable/parent's collider
    //TODO: add drag overriding mechanism to CursorInteractable
    public void cursorInteracting(VectorXZ cursorGlobal) {
        //if (preservedCollider != null) {
        //    print("preseve coll not null");
        //    preservedCollider = client.shouldPreserveConnection();
        //}
        //if (preservedCollider == null) {
        //    client.disconnect();
        //}
    }

    public void endCursorInteraction(VectorXZ cursorGlobal) {
        _cursorInteracting = false;
        connectToColliders();
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
}
