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
        get { return getComponents(); }
    }
    void Awake() {
        _components = GetComponentsInChildren<MonoBehaviour>();
    }

    protected MonoBehaviour[] getComponents() {
        if (_components == null) {
            _components = GetComponentsInChildren<MonoBehaviour>();
        }
        return _components;
    }

    public MonoBehaviour[] componentsOfType<T>() {
        List<MonoBehaviour> result = new List<MonoBehaviour>();
        foreach (MonoBehaviour mb in getComponents()) {
            if (mb is T) {
                result.Add(mb);
            }
        }
        return result.ToArray();
    }
}
