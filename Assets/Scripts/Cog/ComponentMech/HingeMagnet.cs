using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Ideally wouldn't exists but...
 * this class is convenient for using the 
 * 'altWeightedLook' only on HingeJoint magnets
 *  */
public class HingeMagnet : Magnet {

    RingBuffer<VectorXZ> recentLooks = new RingBuffer<VectorXZ>(12);

    public override void Awake() {
        base.Awake();
    }

    protected override void beInfluencedBy(Magnet other) {
        if (Angles.VerySmall(other.getPower())) { return; } //WANT?
        VectorXZ weightedLookXZ = altWeightedTorqueLookFrom(other);
        recentLooks.put(weightedLookXZ);

        /* try to make hinge a little less 'frantic' */
        foreach (VectorXZ look in recentLooks) {
            weightedLookXZ = Vector3.Slerp(weightedLookXZ.vector3(), look.vector3(), .5f);
        }

        Vector3 torque = Angles.radialVectorsToTorqueXZ(localNorth, weightedLookXZ, rb, Time.fixedDeltaTime) * other.getReversed();
        
        if (!Angles.containsNaN(torque)) {
            rb.AddTorque(torque, ForceMode.Impulse);
        }
    }

}
