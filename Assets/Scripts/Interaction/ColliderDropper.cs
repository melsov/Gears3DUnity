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
        Bug.contractLog(name + " on tr enter " + other.name);
        if (client.isCursorInteracting()) {
            if (!colliders.Contains(other)) {
                client.handleTriggerEnter(other);
                Bug.contractLog("adding collider " + other.name);
                colliders.Add(other);
            }
        }
    }

//TODO: with linear actuator, there are a whole bunch of 
// unintended behaviors surrounding droppers, cursor agents
    void OnTriggerExit(Collider other) {
        if (colliders.Contains(other)) {
            Bug.contractLog("removing collider " + other.name);
            colliders.Remove(other);
        } else if (client.isCursorInteracting()) {
            if (!escapedFromColliders.Contains(other)) {
                escapedFromColliders.Add(other);
            }
        }
        try {
            if (client.isCursorInteracting()) {

                if (Cog.FindCog(other.transform)) {
                    //print("client handle trigger exit with cog: " + Cog.FindCog(other.transform).name);
                }
                if (other.transform.parent) {
                    //print("handle with coll parent: " + other.transform.parent.name);
                }
                //print("calling client handle trigger exit w collider: " + other.name);

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
