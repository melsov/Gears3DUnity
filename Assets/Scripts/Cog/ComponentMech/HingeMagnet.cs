using UnityEngine;
using System.Collections;

/*
 * Ideally wouldn't exists but...
 * it's convenient for using the 'altWeightedLook' method only on HingeJoint magnets
 *  */
public class HingeMagnet : Magnet {

    protected override void beInfluencedBy(Magnet other) {
        VectorXZ weightedLookXZ = altWeightedTorqueLookFrom(other);
        Vector3 torque = Angles.radialVectorsToTorqueXZ(localNorth, weightedLookXZ, rb, Time.fixedDeltaTime);

        //TODO: we'd like the alt torque look to be the only one we use (go mainstream). 
        //but it frequently yields NaN torques; debug the Angles method to see what is happening

        if (!Angles.containsNaN(torque)) {
            rb.AddTorque(torque, ForceMode.Impulse);
        }
    }

}
