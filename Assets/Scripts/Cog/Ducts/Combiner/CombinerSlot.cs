using UnityEngine;
using System;
using System.Collections.Generic;

public class CombinerSlot : MonoBehaviour {

    protected List<Combinable> combinables = new List<Combinable>();

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
            combinables.Add(combinable);
            _count++;
        }
        combiner.evaluate();
    }

    public TypeAmount typeAmount {
        get { return new TypeAmount(_type, count); }
    }

    public void release() {
        combinables.RemoveRange(0, combinables.Count);
        _type = null;
        _count = 0;
    }
    
}
