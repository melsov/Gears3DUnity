using UnityEngine;
using System.Collections;
using System;

public class Dispenser : Drivable {

    public Dispensable item;
    public Transform spawnPlatform;
    public float fireRate = .5f;
    public float ejectForce = 4f;
    protected bool shouldDispense;
    protected float timer;
    protected bool hasBuiltInButton;

    // Use this for initialization
    protected override void awake() {
        base.awake();
        ControllerAddOn cao = GetComponentInChildren<ControllerAddOn>();
        if (cao != null) {
            print("connect to cao? hasBuiltInButton is (already) : " + hasBuiltInButton);
            cao.connectTo(GetComponent<Collider>());
            hasBuiltInButton = true;
        }
    }

    protected override bool connectToControllerAddOn(ControllerAddOn cao) {
        if (hasBuiltInButton) { return false; }
        return base.connectToControllerAddOn(cao);
    }

    protected float _power = 1f;
    public float power {
        get { return _power; }
        set {
            if (Time.fixedTime - timer > fireRate) {
                shouldDispense = true;
                _power = 1f;// Mathf.Clamp(value, 0f, 1f);
                timer = Time.fixedTime;
            }
        }
    }

    //protected virtual float interval {
    //    get {
    //        if (power < Mathf.Epsilon) {
    //            return float.MaxValue;
    //        }
    //        return baseFrequency / power;
    //    }
    //}

    public override float driveScalar() {
        return 0f;
    }

    public override Drive receiveDrive(Drive drive) {
        return drive;
    }

    protected virtual Vector3 dispenseDirection {
        get { return new VectorXZ(spawnPlatform.transform.position - transform.position).vector3(0f).normalized;  }
    }


    protected override void update() {
        base.update();
        if (shouldDispense) {
            shouldDispense = false;
            dispense();
        }
    }
    
    protected virtual void dispense() {
        Dispensable d = Instantiate<Dispensable>(item);
        d.enabled = true;
        d.transform.position = spawnPlatform.position;
        d.GetComponent<Rigidbody>().AddForce(dispenseDirection * ejectForce, ForceMode.Impulse);
    }

    protected override void handleAddOnScalar(float scalar) {
        power = scalar;
    }
}
