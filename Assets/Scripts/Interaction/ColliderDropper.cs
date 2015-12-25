using UnityEngine;
using System.Collections.Generic;

public class ColliderDropper : MonoBehaviour {
    
    public List<Collider> colliders = new List<Collider>();
    private ColliderDropperClient client;

    void Awake() {
        client = GetComponent<ColliderDropperClient>();
    }

    void OnTriggerEnter(Collider other) {
        if (client.cursorInteracting()) {
            if (!colliders.Contains(other)) {
                highlight(other, true);
                colliders.Add(other);
            }
        }
    }

    void OnTriggerExit(Collider other) {
        highlight(other, false);
        colliders.Remove(other);
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

public interface ColliderDropperClient
{
    bool cursorInteracting();
}
