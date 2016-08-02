using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

public class CombinerMultiSlot : CombinerSlot
{
    private SpriteRenderer[] spriteRenderers;
    private CombinableCollection collection;

    protected override List<Combinable> _combinables {
        get { return collection; }
    }

    public override void addCombinable(Combinable combinable) {
        if (collection.full) {
            Destroy(combinable.gameObject);
            return;
        }
        collection.add(combinable);
        combinable.disable();
        setIcons();
        combiner.evaluate();
    }

    private void setIcons() {
        for(int i = 0; i < spriteRenderers.Length; ++i) {
            Combinable c = collection[i];
            SpriteRenderer sr = spriteRenderers[i];
            if (!c) {
                sr.sprite = emptySprite;
            } else {
                sr.sprite = c.sprite;
            }
        }
    }

    public override void Awake() {
        base.Awake();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        collection = new CombinableCollection(this);
        setIcons();
        StartCoroutine(testCombiner());
    }

    private IEnumerator testCombiner() {
        Transform orig = Resources.LoadAll<Anvil>("Prefabs/Dispensables")[0].transform;
        for(int i = 0; i < 2; ++i) {
            Transform r = Instantiate<Transform>(orig);
            yield return new WaitForSeconds(1f);
            r.position = transform.position;
        }
    }

//TODO: for test, just destroy & release when collection becomes full
// could just artificially create and add combinables in a coroutine also
// test display?

    public override void clear() {
        collection.clear();
        setIcons();
    }

    protected class CombinableCollection : IEnumerable<Combinable> {
        private Combinable[] combinables;
        private WeakReference _combinerMultiSlot;
        private CombinerMultiSlot combinerMultiSlot {
            get { return (CombinerMultiSlot)_combinerMultiSlot.Target; }
        }
        private int index;

        public CombinableCollection(CombinerMultiSlot cms) {
            _combinerMultiSlot = new WeakReference(cms);
            combinables = new Combinable[combinerMultiSlot.spriteRenderers.Length];
        }

        public bool full { get { return index >= combinables.Length; } }

        public Combinable this[int i] {
            get {
                if (i < 0 || i >= combinables.Length) { return null; }
                return combinables[i];
            }
        }

        public int Length { get { return combinables.Length; } }

        public void add(Combinable c) {
            if (full) {
                throw new Exception("CombinableSet is at capacity. cant add");
            }
            combinables[index++] = c;
        }

        public void clear() {
            for(int i = 0; i < combinables.Length; ++i) {
                if (combinables[i]) {
                    Destroy(combinables[i].gameObject);
                }
                combinables[i] = null;
            }
            index = 0;
        }

        public IEnumerator<Combinable> GetEnumerator() {
            foreach(Combinable c in combinables) { yield return c; }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public static implicit operator List<Combinable> (CombinableCollection cc) {
            if (cc == null) { return new List<Combinable>(); }
            List<Combinable> result = new List<Combinable>();
            foreach(Combinable c in cc.combinables) {
                if (c != null) {
                    result.Add(c);
                }
            }
            return result;
        }
    }

    internal OrderedRecipe getOrderedRecipe() {
        List<Combinable> list = collection;
        return new OrderedRecipe(list.ToArray());
    }
}




