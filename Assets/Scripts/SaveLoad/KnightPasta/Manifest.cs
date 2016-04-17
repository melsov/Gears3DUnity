using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
Maintains a set of all mono behaviours attached
a prefab originally. Useful for distinguishing 
'integral' components from children that were 
attached at runtime.
*/
public class Manifest : MonoBehaviour {

    private MonoBehaviour[] _components;
    public MonoBehaviour[] prefabComponents {
        get { return _components; }
    }
    void Awake() {
        _components = GetComponentsInChildren<MonoBehaviour>();
    }

    public MonoBehaviour[] componentsOfType<T>() {
        List<MonoBehaviour> result = new List<MonoBehaviour>();
        foreach (MonoBehaviour mb in _components) {
            if (mb is T) {
                result.Add(mb);
            }
        }
        return result.ToArray();
    }
}
