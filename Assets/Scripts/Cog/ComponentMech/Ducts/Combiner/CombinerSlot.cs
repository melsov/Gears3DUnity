using UnityEngine;
using System;
using System.Collections.Generic;

public class CombinerSlot : MonoBehaviour {

    private List<Combinable> __combinables = new List<Combinable>();
    protected virtual List<Combinable> _combinables {
        get { return __combinables; }
    }
    public IEnumerable<Combinable> combinables() {
        foreach (Combinable com in _combinables) {
            yield return com;
        }
    }
    private TextMesh _itemCountText;
    protected TextMesh itemCountText {
        get {
            if (!_itemCountText) {
                _itemCountText = GetComponentInChildren<TextMesh>();
            }
            return _itemCountText;
        }
    }
    protected SpriteRenderer icon;
    protected Sprite emptySprite;

    protected Type _type;
    protected int _count;
    protected void setCount(int c) {
        _count = c;
        itemCountText.text = "" + _count;
    }
    public int count { get { return _count; } }
    protected WeakReference _combiner = new WeakReference(null);
    protected Combiner combiner { get { return (Combiner)_combiner.Target; } }

    public virtual void Awake() {
        _combiner = new WeakReference(GetComponentInParent<Combiner>());
        icon = GetComponentInChildren<SpriteRenderer>();
        emptySprite = icon.sprite;
        boxColliderToDispensableLayer();
    }

    private void boxColliderToDispensableLayer() {
        BoxCollider bc = GetComponent<BoxCollider>();
        if (!bc) { throw new Exception("Combiner slot needs a box collider. What gives."); }
        bc.center = TransformUtil.SetY(bc.center, YLayer.dispenseable);
    }

    public virtual void addCombinable(Combinable combinable) {
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

    public virtual void clear() {
        _combinables.Clear();
        _type = null;
        icon.sprite = emptySprite;
        setCount(0);
    }
    
}
