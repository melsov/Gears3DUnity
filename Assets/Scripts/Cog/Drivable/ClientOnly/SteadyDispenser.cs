using UnityEngine;
using System.Collections;
using System;

public class SteadyDispenser : Drivable {

    public Dispensable item;
    public Transform spawnPlatform;
    public float baseFrequency = 2f;
    public float ejectForce = 4f;
    protected float timer;

    protected float _power = 1f;
    public float power {
        get { return _power; }
        set { _power = Mathf.Clamp(value, 0f, 1f); }
    }

    protected virtual float interval {
        get {
            if (power < Mathf.Epsilon) {
                return float.MaxValue;
            }
            return baseFrequency / power;
        }
    }

    public override float driveScalar() {
        return 0f;
    }

    public override Drive receiveDrive(Drive drive) {
        return drive;
    }

    protected virtual Vector3 dispenseDirection {
        get { return new VectorXZ(spawnPlatform.transform.position - transform.position).vector3(0f).normalized;  }
    }

    // Use this for initialization
    protected override void awake() {
        base.awake();
    }

    protected override void update() {
        base.update();
        timer += Time.deltaTime;
        if (timer > interval) {
            timer = 0f;
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

    public override ConnectionSiteAgreement.ConnektAction connektActionAsTravellerFor(ContractSpecification specification) {
        throw new NotImplementedException();
    }

    protected override UniqueClientContractSiteBoss getUniqueClientSiteConnectionSiteBoss() {
        throw new NotImplementedException();
    }
}
