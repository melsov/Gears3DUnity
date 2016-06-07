using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NeighborColliderLookup : MonoBehaviour {

    protected Collider _collider;
    protected HashSet<Collider> overlappingColliders = new HashSet<Collider>();
    protected ColliderSet colliderSet;

    public void Awake() {
        _collider = GetComponent<Collider>();
        if (_collider == null) {
            if (GetComponent<ICursorAgentClient>() != null) {
                _collider = GetComponent<ICursorAgentClient>().mainCollider();
            }
        }
        UnityEngine.Assertions.Assert.IsTrue(_collider.isTrigger, "Need a trigger collider in " + name);
        colliderSet = GetComponent<ColliderSet>();
    }

    public List<Collider> overlappingNonChildColliders() {
        List<Collider> result = new List<Collider>();
        foreach(Collider c in overlappingColliders) {
            if (!colliderSet.contains(c)) {
                result.Add(c);
            }
        }
        Debug.LogError("overlapping count: " + result.Count);
        return result;
    }

    public HashSet<T> overlappingInCog<T>() where T : MonoBehaviour
    {
        HashSet<T> result = new HashSet<T>();
        foreach(Collider c in overlappingColliders) {
            T item = Cog.FindInCog<T>(c.transform);
            if (item == null) { continue; }
            print("found overlapping " + item.name);
            result.Add(item);
        }
        Debug.LogError("overlapping count: " + result.Count);
        return result;
    }

    public void OnTriggerEnter(Collider other) {
        if (GetComponent<Gear>()) { print("gear t enter: with: " + other.name); }
        overlappingColliders.Add(other);
    }

    public void OnTriggerExit(Collider other) {
        overlappingColliders.Remove(other);
    }
}
