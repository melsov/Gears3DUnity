using UnityEngine;
using System.Collections;
using System;

public class ControllerAddOn : AddOn {

    public delegate void SetScalar(float scalar);
    public SetScalar setScalar;

    protected override void awake() {
        base.awake();
    }
}

public interface IControllerAddOnProvider
{
    ControllerAddOn getControllerAddOn();
}



