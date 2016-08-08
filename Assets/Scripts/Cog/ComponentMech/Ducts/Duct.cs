using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

//TODO: move contract stuff to Placeable
public class Duct : MonoBehaviour
{

    public void Awake() {
        awake();
    }

    protected virtual void awake() {
        transform.position = TransformUtil.SetY(transform.position, YLayer.dispenseable);
    }
}
