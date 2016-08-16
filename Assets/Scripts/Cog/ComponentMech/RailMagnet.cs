using UnityEngine;
using System.Collections;

public class RailMagnet : Magnet {

    [SerializeField]
    protected LineSegment lineSegment;

    protected override void beInfluencedBy(Magnet other) {
        VectorXZ northToSouth = northToSouthOf(other).normalized * forceFrom(other);
        VectorXZ weightedLookXZ = weightedTorqueLookFrom(other); 

        VectorXZ tangent = weightedLookXZ - localNorth;
        VectorXZ linearXZ = northToSouth - tangent;
        linearXZ = lineSegment.dotWithNormalized(linearXZ) * lineSegment.normalized;
        Vector3 lin = linearXZ.vector3() * other._reversed;
        Vector3 predictedPos = rb.position + TransformUtil.distanceOneFrameGiven(rb, lin);
        if (lineSegment.isOnSegment(predictedPos)) {
            rb.AddForce(lin , ForceMode.Force);
        } else {
            rb.MovePosition(lineSegment.closestPointOnSegment(predictedPos).vector3());
        }
    }

}
