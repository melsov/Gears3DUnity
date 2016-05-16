using UnityEngine;
using System.Collections;

public abstract class GearDrivenMechanism : Gear {
    protected Transform gearMesh;

    protected override void awake() {
        base.awake();
        foreach(Transform t in GetComponentsInChildren<Transform>()) {
            if (t.name.Equals("GearMesh")) {
                gearMesh = t;
            }
        }
    }

    public override Drive receiveDrive(Drive drive) {
        Drive baseDrive = base.receiveDrive(drive);
        updateMechanism(baseDrive);
        return baseDrive;
    }

    protected abstract void updateMechanism(Drive drive);

    protected override DrivableConnection getDrivableConnection(Collider other) {
        if (FindInCog<Gear>(other.transform) != null) {
            return base.getDrivableConnection(other);
        }
        return new DrivableConnection(this);
    }

    protected override Transform gearTransform {
        get {
            return gearMesh;
        }
    }

	
}
