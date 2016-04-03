using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

public class ColliderDropper : MonoBehaviour {
    
    public List<Collider> colliders = new List<Collider>();
    private IColliderDropperClient client;

    void Awake() {
        client = GetComponent<IColliderDropperClient>();
        if (client == null) {
            client = GetComponentInParent<IColliderDropperClient>();
            Assert.IsTrue(client != null, "no collider dropper client?");
        }
    }

    void OnTriggerEnter(Collider other) {
        if (client.isCursorInteracting()) {
            if (!colliders.Contains(other)) {
                highlight(other, true);
                colliders.Add(other);
            }
        }
    }

//TODO: with linear actuator, there are a whole bunch of 
// unintended behaviors surrounding droppers, cursor agents
    void OnTriggerExit(Collider other) {
        try {
            highlight(other, false);
        } catch(System.Exception e) {
            print(other.name + " didnt have mat [o]?");
            Bug.bugAndPause(e.ToString());
        }
        colliders.Remove(other);
        if (client.isCursorInteracting()) {
            client.handleTriggerExit(other);
        }
    }

    public void removeAll() {
        colliders.RemoveRange(0, colliders.Count);
    }
    
    private void highlight(Collider other, bool wantHighlight) {
        Highlighter h = other.GetComponent<Highlighter>();
        if (h == null) return;
        if (wantHighlight) {
            h.highlight();
        } else {
            h.unhighlight();
        }
    }

}

public interface IColliderDropperClient
{
    bool isCursorInteracting();
    void handleTriggerExit(Collider other);
}
