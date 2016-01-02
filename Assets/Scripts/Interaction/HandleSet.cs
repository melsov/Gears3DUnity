using UnityEngine;
using System.Collections;

public class HandleSet : MonoBehaviour {

    private Handle[] _handles;
    public Handle[] handles {
        get {
            if (_handles == null) {
                _handles = GetComponentsInChildren<Handle>();
            }
            return _handles;
        }
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
