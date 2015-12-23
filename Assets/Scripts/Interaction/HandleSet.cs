using UnityEngine;
using System.Collections;

public class HandleSet : MonoBehaviour {

    public Handle[] handles;

    void Awake() {
        handles = GetComponentsInChildren<Handle>();
    }

    public bool contains(Handle h) {
        foreach (Handle handle in handles) { 
            if (handle == h) {
                return true;
            }
        }
        return false;
    }

    public Handle getAnotherThatIsntThisOne(Handle h) {
        if (h == null) return null;
        if (handles.Length > 1 && contains(h)) {
            foreach (Handle handle in handles) { 
                if (handle != h) {
                    return handle;
                }
            }
        }
        return null;
    }
}
