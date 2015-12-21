using UnityEngine;
using System.Collections.Generic;

public class ColliderDropper : MonoBehaviour {
    //TODO: decide whether anyone needs this: maybe require users to implement an interface ColliderHandler
    
    public List<Collider> colliders = new List<Collider>();
    private ColliderDropperClient client;

    void Awake() {
        client = GetComponent<ColliderDropperClient>();
    }

    void OnTriggerEnter(Collider other) {
        if (client.cursorInteracting()) {
            if (!colliders.Contains(other)) {
                colliders.Add(other);
            }
        }
    }

    void OnTriggerExit(Collider other) {
        colliders.Remove(other);
    }

    public void removeAll() {
        colliders.RemoveRange(0, colliders.Count);
    }
}

public interface ColliderDropperClient
{
    bool cursorInteracting();
}
