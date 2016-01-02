using UnityEngine;
using System.Collections;
using System;

public class Handle : MonoBehaviour  {

    public Transform widget;

    void Awake() {
        awake();
    }
    protected virtual void awake() {
        gameObject.layer = LayerLookup.DragOverride;
    }
    //public delegate bool IsCursorInteracting();
    //public IsCursorInteracting dIsCursorInteracting;
}
