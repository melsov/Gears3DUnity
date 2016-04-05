using UnityEngine;
using System.Collections;

public class LinearActuatorConstraint : Constraint
{
    protected Vector3 prevTargetPosition = Vector3.zero;
    protected int intersectionIndex = -1;
    private bool needToConfigure = true;

    protected override void awake() {
        base.awake();
    }

    public virtual bool isTargetInRange() {
        return isTargetInRange(false);
    }

    public virtual bool isTargetInRange(bool extendedRange) {
        if (constraintTarget.lineSegmentReference == null) return false;
        Vector3 rangeV = extendedRange ? (poleDirection * 1.25f) : poleDirection; //TODO: make more precise
        return 
            (constraintTarget.lineSegmentReference.start.position - constraintTarget.reference.position).sqrMagnitude < rangeV.sqrMagnitude ||
            (constraintTarget.lineSegmentReference.end.position - constraintTarget.reference.position).sqrMagnitude < rangeV.sqrMagnitude;
    }

    protected Vector3 poleDirection {
        get { return constraintTarget.altReference.position - constraintTarget.reference.position; }
    }

    public override void configure() {
        base.configure();
        if (constraintTarget.driverReference == null) {
            print("null driver ref in linear actuator constraint"); return;
        }
        if (constraintTarget.drivenReference == null ||  !(constraintTarget.drivenReference is LinearActuator)) {
            print("wrong kind of drivable: " + constraintTarget.drivenReference.name); return;
        }
        if(!reignInLineSegment()) {
            return;
        }
        chooseIntersectionIndex();
        extendLineSegment();
        needToConfigure = false;
    }

    private bool reignInLineSegment() {
        Gear gear = constraintTarget.driverReference.GetComponent<Gear>();
        if (gear == null) { return false; }
        VectorXZ gearCenter = new VectorXZ(gear.transform.position);
        float radius = (gearCenter.vector3() - constraintTarget.reference.position).magnitude;
        float poleDistance = new VectorXZ(poleDirection).magnitude;
        LinearActuator la = constraintTarget.lineSegmentReference.GetComponentInParent<LinearActuator>();
        if (la == null) { return false; }
        foreach (VectorXZ direction in Angles.UnitVectors(36)) 
        {
            VectorXZ gearRim = gearCenter + direction * radius;
            VectorXZ closestOnSegment = constraintTarget.lineSegmentReference.closestPointOnSegment(gearRim);
            VectorXZ dif = closestOnSegment - gearRim;
            if (dif.magnitudeSquared > poleDistance * poleDistance)
            {
                VectorXZ nudge = dif.normalized * -1f * (dif.magnitude - poleDistance);
                la.transform.position += nudge.vector3();
            }
        }
        return true;
    }

    //private void adjustPosition() {
    //    Gear gear = constraintTarget.driverReference.GetComponent<Gear>();
    //    VectorXZ center = new VectorXZ(gear.transform.position);
    //    VectorXZ closest = constraintTarget.lineSegmentReference.closestPointOnLine(center);
    //    float radius = gear.radiusInDirection(closest.vector3() - gear.transform.position);
    //    float minDistanceFromCenter =  poleDirection.magnitude - radius;
    //    float closestDistFromCenter = (closest - center).magnitude;
    //    if (minDistanceFromCenter > closestDistFromCenter && constraintTarget.lineSegmentReference.isOnSegment(closest)) {
    //        LinearActuator la = constraintTarget.target.GetComponentInParent<LinearActuator>();
    //        VectorXZ dif = closest - center;
    //        VectorXZ nudge = center + dif.normalized * minDistanceFromCenter - closest;
    //        la.transform.position += nudge.vector3();
    //    }
    //}

    private void chooseIntersectionIndex() {
        Gear gear = constraintTarget.driverReference.GetComponent<Gear>();
        if (gear == null) { return; }

        //survey line segment
        Vector3 gearCenter = gear.transform.position;
        float radius = (gearCenter - constraintTarget.reference.position).magnitude;
        int zeroHit = 0, oneHit = 0;

        foreach (VectorXZ direction in Angles.UnitVectors(36)) 
        {
            Vector3 testDirection = (direction * radius).vector3();
            int result = connectedIndicesFromCenter(gearCenter + testDirection);
            if (result == 2) {
                zeroHit++;
                oneHit++;
            } else if (result == 0) {
                zeroHit++;
            } else if (result == 1) {
                oneHit++;
            }
        }
        if (zeroHit > oneHit) {
            intersectionIndex = 0;
        } else {
            intersectionIndex = 1;
        }
    }

    private void extendLineSegment() {
        Gear gear = constraintTarget.driverReference.GetComponent<Gear>();
        VectorXZ gearCenter = new VectorXZ(gear.transform.position);
        float radius = (gearCenter.vector3() - constraintTarget.reference.position).magnitude;

        foreach (VectorXZ dir in Angles.UnitVectors(36)) {
            VectorXZ gearRim = gearCenter + dir * radius;
            VectorXZ point = intersectionPoint(gearRim.vector3(), constraintTarget.altReference.position, constraintTarget.lineSegmentReference);

            if (!point.isFakeNull()) {
                VectorXZ dif = point - constraintTarget.lineSegmentReference.startXZ;
                float dot = constraintTarget.lineSegmentReference.normalized.dot(dif.normalized);
                constraintTarget.lineSegmentReference.extendToAccommodate(point + (constraintTarget.lineSegmentReference.normalized * dot * 1.1f) );
            }

        }
    }

    private int connectedIndicesFromCenter(Vector3 center) {
        VectorXZ[] points = new VectorXZ[2];
        bool intersected = intersectionPoints(ref points, constraintTarget.reference.position, constraintTarget.altReference.position, constraintTarget.lineSegmentReference);
        int result = -1;
        bool zedOnSeg = constraintTarget.lineSegmentReference.isOnSegment(points[0]);
        bool oneOneSeg = constraintTarget.lineSegmentReference.isOnSegment(points[1]);
        if (zedOnSeg && oneOneSeg) {
            result = 2;
        } else if (zedOnSeg) {
            result = 0;
        } else if (oneOneSeg) {
            result = 1;
        }
        return result;
    }

    protected override void constrain() {
        if (constraintTarget.target == null) {
            return;
        }
        if(needToConfigure) {
            configure();
        }

        Vector3 curDirection = poleDirection; 
        Vector3 target = constraintTarget.target.position;
        Vector3 nudge = constraintTarget.reference.position - prevTargetPosition;

        if (nudge.sqrMagnitude > 0f) {
            VectorXZ[] points = new VectorXZ[2];
            VectorXZ n = new VectorXZ(nudge);

            bool intersected = intersectionPoints(ref points, constraintTarget.reference.position, constraintTarget.altReference.position, constraintTarget.lineSegmentReference);

            if (intersected) { 
                target = constraintTarget.lineSegmentReference.closestPointOnSegment(points[intersectionIndex]).vector3(target.y);
            }
        }
        Vector3 nextDirection = target - constraintTarget.reference.position;
        transform.RotateAround(constraintTarget.reference.position, EnvironmentSettings.towardsCameraDirection, Quaternion.FromToRotation(curDirection, nextDirection).eulerAngles.y);
        prevTargetPosition = constraintTarget.reference.position;
    }

    private VectorXZ intersectionPoint(Vector3 center, Vector3 pointOnCircle, LineSegment lineSegment) {
        VectorXZ[] points = new VectorXZ[2];
        if(intersectionPoints(ref points,center,pointOnCircle,lineSegment)) {
            return points[intersectionIndex];
        }
        return VectorXZ.fakeNull;
    }

    private static bool intersectionPoints(ref VectorXZ[] points, Vector3 center, Vector3 pointOnCircle, LineSegment lineSegment) {
        // p and q are x and y offsets for circle defined by r = pole length, center = pinned down end of pole (a.k.a. reference.position)
        float p = center.x; 
        float q = center.z; 
        VectorXZ pole = new VectorXZ(pointOnCircle - center);
        float slope = lineSegment.slopeXZ;
        float intercept = lineSegment.interceptXZ;

        //coefficients
        float a = slope*slope + 1;
        float b = 2 * (slope * intercept - slope * q - p);
        float c = (q * q - pole.magnitudeSquared + p * p - 2 * intercept * q + intercept * intercept);
        float discriminant = b * b - 4 * a * c;

        if (discriminant > 0f) {
            float disSqrt = Mathf.Sqrt(discriminant);
            float x1 = (-b + disSqrt) / (2 * a);
            float y1 = slope * x1 + intercept;
            float x2 = (-b - disSqrt) / (2 * a);
            float y2 = slope * x2 + intercept;
            points[0] = new VectorXZ(x1, y1);
            points[1] = new VectorXZ(x2, y2);
            return true;
        } else if (-1f * discriminant < Mathf.Epsilon) {
            float x = -b / (2 * a);
            float y = slope * x + intercept;
            VectorXZ tangent = new VectorXZ(x, y);
            points[0] = tangent;
            points[1] = tangent;
            return true;
        }
        return false;
    }

    private VectorXZ pointOnLineSegment(VectorXZ p) {
        return constraintTarget.lineSegmentReference.closestPointOnSegment(p);
    }

    private static bool SameSign(float a, float b) {
        return NearlyEqual(Mathf.Sign(a), Mathf.Sign(b));
    }

    private static bool NearlyEqual(float a, float b) {
        if (a == b) { return true; }
        return Mathf.Abs(a - b) < Mathf.Epsilon;
    }

}
