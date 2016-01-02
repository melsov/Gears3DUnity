using UnityEngine;
using System.Collections;

public class ReceiverAddOn : AddOn {

    protected float _input;
    public virtual float input {
        get { return _input; }
        set { _input = value; }
    }

}
