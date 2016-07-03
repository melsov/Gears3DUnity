using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class FiniteSet<T> : IEnumerable<T> {

    private int limit;
    private HashSet<T> storage = new HashSet<T>();

    public FiniteSet(int limit) {
        this.limit = limit;
    }

    public bool Add(T a) {
        if (storage.Count < limit) {
            storage.Add(a);
            return true;
        }
        return false;
    }

    public bool Contains(T a) {
        return storage.Contains(a);
    }

    public bool full {
        get { return storage.Count >= limit; }
    }


    public IEnumerator<T> GetEnumerator() {
        foreach(T a in storage) {
            yield return a;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}
