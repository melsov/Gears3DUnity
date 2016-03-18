using UnityEngine;
using System;
using System.Collections.Generic;

public class CombinerSlot : MonoBehaviour {

    protected List<Combinable> _combinables = new List<Combinable>();
    public IEnumerable<Combinable> combinables() {
        foreach (Combinable com in _combinables) {
            yield return com;
        }
    }

    protected Type _type;
    protected int _count;
    public int count { get { return _count; } }
    protected WeakReference _combiner;
    protected Combiner combiner {
        get {
            return (Combiner) _combiner.Target;
        }
        set { _combiner = new WeakReference(value); }
    }

    void Awake() {
        awake();
    }

    protected virtual void awake() {
        combiner = GetComponentInParent<Combiner>();
    }

    public void addCombinable(Combinable combinable) {
        if (_type == null) {
            _type = combinable.GetType();
        }
        if (_type.Equals(combinable.GetType())) {
            combinable.transform.position = transform.position;
            combinable.disable();
            _combinables.Add(combinable);
            _count++;
        }
        combiner.evaluate();
    }

    public bool empty {
        get { return _count == 0; }
    }

    public TypeAmount typeAmount {
        get { return new TypeAmount(_type, count); }
    }

    public void release() {
        _combinables.RemoveRange(0, _combinables.Count);
        _type = null;
        _count = 0;
    }
    
}
