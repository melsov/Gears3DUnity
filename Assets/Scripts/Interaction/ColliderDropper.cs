using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

public class ColliderDropper : MonoBehaviour {
    
    public List<Collider> colliders = new List<Collider>();
    public List<Collider> escapedFromColliders = new List<Collider>();
    private IColliderDropperClient client;

    private Cog knowYourCog;

    void Awake() {
        client = GetComponent<IColliderDropperClient>();
        if (client == null) {
            client = GetComponentInParent<IColliderDropperClient>();
            Assert.IsTrue(client != null, "no collider dropper client?");
        }
        knowYourCog = GetComponent<Cog>();
    }

    void OnTriggerEnter(Collider other) {
        if (client.isCursorInteracting()) {
            if (!colliders.Contains(other)) {
                client.handleTriggerEnter(other);
                colliders.Add(other);
            }
        }
    }

//TODO: with linear actuator, there are a whole bunch of 
// unintended behaviors surrounding droppers, cursor agents
    void OnTriggerExit(Collider other) {
        if (colliders.Contains(other)) {
            colliders.Remove(other);
        } else if (client.isCursorInteracting()) {
            if (!escapedFromColliders.Contains(other)) {
                escapedFromColliders.Add(other);
            }
        }
        try {
            if (client.isCursorInteracting()) {
                client.handleTriggerExit(other);
            }
        } catch(System.NullReferenceException nre) {
            Debug.LogError(((knowYourCog != null) ? knowYourCog.name + "'s coll dropper got a null RE" : " coll dropper with no cog got a null RE") + nre.StackTrace);
        }
    }

    public void removeAll() {
        colliders.RemoveRange(0, colliders.Count);
        escapedFromColliders.RemoveRange(0, escapedFromColliders.Count);
    }
    
    

}

public interface IColliderDropperClient
{
    bool isCursorInteracting();
    void handleTriggerEnter(Collider other);
    void handleTriggerExit(Collider other);
}
