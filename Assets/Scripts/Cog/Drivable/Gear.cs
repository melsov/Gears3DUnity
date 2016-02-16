using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Gear : Drivable  { 

//TODO: account for tooth size 
//TODO: want radius to be a public method of Drivable: radius at position?   

    public override float driveScalar() {
        return _angleStep.deltaAngle * radius;
    }

    protected override bool vConnectTo(Collider other) {
        if (isDriven()) { //TODO: aren't we guaranteed not to be connected at this point?
            return false;
        }
        if (isConnectedTo(other.transform)) { return false; }
        // If this is an axel, get driven by it
        Axel axel = getAxel(other);
        if (axel != null) {
            setSocketClosestToAxel(axel);
            return true;
        }
        if (!isInConnectionRange(other)) {
            return false;
        }
        // If this is a gear, get driven by it
        Gear gear = other.GetComponent<Gear>(); 
        if (gear != null && gear is Drivable) {
            _driver = gear;
            gear.addDrivable(this);
            positionRelativeTo(gear);
            return true;
        }
        return false;
    }


    public override Drive receiveDrive(Drive drive) {
        transform.eulerAngles += new Vector3(0f, drive.amount * -1f / radius, 0f);
        return drive;
    }

    protected override bool connectToControllerAddOn(ControllerAddOn cao) {
        return false;
    }

    protected override bool connectToReceiverAddOn(ReceiverAddOn rao) {
        return false;
    }
}

