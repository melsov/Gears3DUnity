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
    protected TextMesh itemCountText;
    protected SpriteRenderer icon;
    protected Sprite emptySprite;

    protected Type _type;
    protected int _count;
    protected void setCount(int c) {
        _count = c;
        itemCountText.text = "" + _count;
    }
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
        itemCountText = GetComponentInChildren<TextMesh>();
        icon = GetComponentInChildren<SpriteRenderer>();
        emptySprite = icon.sprite;
        setCount(_count);
    }

    public void addCombinable(Combinable combinable) {
        if (_type == null) {
            _type = combinable.GetType();
            if (combinable.sprite != null) {
                icon.sprite = combinable.sprite;
            }
        }
        if (_type.Equals(combinable.GetType())) {
            combinable.transform.position = transform.position;
            combinable.disable();
            _combinables.Add(combinable);
            setCount(count + 1);
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
        icon.sprite = emptySprite;
        setCount(0);
    }
    
}
