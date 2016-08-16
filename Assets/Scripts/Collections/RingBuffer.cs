using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class RingBuffer<T> : IEnumerable<T>
{
    private T[] storage;
    private int index;
    public int available {
        get;
        private set;
    }

    public RingBuffer(int length) {
        storage = new T[length];
    }

    private int capacity { get { return storage.Length; } }

    public void put(T t) {
        if (index >= capacity) {
            index = 0;
        }
        storage[index] = t;
        index++;
        available = available >= capacity ? capacity : ++available;
    }

    public IEnumerator<T> GetEnumerator() {
        int start = index - available;
        if (start < 0) { start += capacity; }
        for (int i = 0; i < available; ++i) {
            yield return storage[(i + start) % capacity];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}
