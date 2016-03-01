using UnityEngine;
using System.Collections;

public class LinearActuatorConstraint : Constraint
{

    public float testFactor = 1.2f;
    //public LineSegment lineSegment;
    protected Vector3 prevTargetPosition = Vector3.zero;
    protected int intersectionIndex = -1;

    protected override void awake() {
        base.awake();
        prevTargetPosition = constraintTarget.reference.position;
    }

    public override void configure(Drivable referenceDrivable, Drivable targetDrivable) {
        base.configure(referenceDrivable, targetDrivable);
        if (targetDrivable == null || !(targetDrivable is LinearActuator)) {
            print("wrong kind of drivable: " + targetDrivable.name); return;
        }

        chooseIntersectionIndex(referenceDrivable);
    }

    private void chooseIntersectionIndex(Drivable referenceDrivable) {
        Gear gear = referenceDrivable.GetComponent<Gear>();
        if (gear == null) {
            print("bad reference drivable: " + referenceDrivable.name); return;
        }

        //survey line segment
        int sections = 36;
        Vector3 gearCenter = gear.transform.position;
        float radius = (gearCenter - constraintTarget.reference.position).magnitude;
        int zeroHit = 0, oneHit = 0;
        for (int i=0; i<sections; ++i) {
            float ang = Mathf.PI * 2 * i / ((float)sections);
            Vector3 testDirection = new Vector3(radius * Mathf.Cos(ang), 0f, radius * Mathf.Sin(ang));
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

    //CONSIDER: THIS SHOULD REALLY BE CALLED 'LINESEGMENT CONSTRAINT'
    protected override void constrain() {
        if (constraintTarget.target == null) {
            return;
        }

        Vector3 curDirection = constraintTarget.altReference.position - constraintTarget.reference.position;
        Vector3 target = constraintTarget.target.position;
        Vector3 nudge = constraintTarget.reference.position - prevTargetPosition; // constraintTarget.target.rotation.eulerAngles * testFactor; // Dot(constraintTarget.target.rotation.eulerAngles, curDirection.normalized) * .3f * constraintTarget.target.rotation.eulerAngles;

        if (nudge.sqrMagnitude != 0f) {
            VectorXZ[] points = new VectorXZ[2];
            VectorXZ n = new VectorXZ(nudge);

            bool intersected = intersectionPoints(ref points, constraintTarget.reference.position, constraintTarget.altReference.position, constraintTarget.lineSegmentReference);
            BugLine.Instance.markPoint(points[0], 0);
            BugLine.Instance.markPoint(points[1], 1);
            target = points[intersectionIndex].vector3(target.y);

            if (false && intersected) { //UNREACHABLE
                //if p0 == p1??
                //TODO: this algo is no good! find the definitive way
                bool zedOnSeg = constraintTarget.lineSegmentReference.isOnSegment(points[0]);
                bool oneOneSeg = constraintTarget.lineSegmentReference.isOnSegment(points[1]);
                if (zedOnSeg && oneOneSeg) { // uh-oh? TODO: figure definitive way of knowing
                    // hack?
                    float dist0 = (new VectorXZ(constraintTarget.altReference.position) - points[0]).magnitudeSquared;
                    float dist1 = (new VectorXZ(constraintTarget.altReference.position) - points[1]).magnitudeSquared;
                    if (dist0 < dist1) {
                        target = pointOnLineSegment(points[0]).vector3(target.y);
                    } else {
                        target = pointOnLineSegment(points[1]).vector3(target.y);
                    }
                } else if (zedOnSeg) {
                    target = pointOnLineSegment(points[0]).vector3(target.y);
                } else if (oneOneSeg) {
                    target = pointOnLineSegment(points[1]).vector3(target.y);
                }

                //// choose the point that 'nudge' points towards
                //VectorXZ tXZ = new VectorXZ(target);
                //float nudgeDotPole = constraintTarget.lineSegmentReference.distance.dot(n);
                //float dirP0 = constraintTarget.lineSegmentReference.distance.dot(points[0] - tXZ);
                //float dirP1 = constraintTarget.lineSegmentReference.distance.dot(points[1] - tXZ);

                //if (SameSign(nudgeDotPole, dirP0)) {
                //    target = pointOnLineSegment(points[0]).vector3(target.y);
                //} else if (SameSign(nudgeDotPole, dirP1)) {
                //    target = pointOnLineSegment(points[1]).vector3(target.y);
                //} 
            } else {
                //float dot = constraintTarget.lineSegmentReference.normalized.dot(n) * testFactor;
                //n = constraintTarget.lineSegmentReference.normalized * dot;
                //target = target + n.vector3();
            }
        }
        Vector3 nextDirection = target - constraintTarget.reference.position;
        transform.RotateAround(constraintTarget.reference.position, EnvironmentSettings.towardsCameraDirection, Quaternion.FromToRotation(curDirection, nextDirection).eulerAngles.y);

        prevTargetPosition = constraintTarget.reference.position;
    }
    // Called from fixed update

    private static int tick = 0;

    private static bool intersectionPoints(ref VectorXZ[] points, Vector3 center, Vector3 pointOnCircle, LineSegment lineSegment) {

        // p and q are x and y offsets for circle defined by r = pole length, center = pinned down end of pole (reference.position)
        float p = center.x; // constraintTarget.reference.position.x;
        float q = center.z; // constraintTarget.reference.position.z;
        VectorXZ pole = new VectorXZ(pointOnCircle - center); // constraintTarget.altReference.position - constraintTarget.reference.position);
        float slope = lineSegment.slopeXZ; // constraintTarget.lineSegmentReference.slopeXZ;
        float intercept = lineSegment.interceptXZ; // constraintTarget.lineSegmentReference.interceptXZ;

        if (tick++ > 30) {
            tick = 0;
            //BugLine.Instance.markPoint(new VectorXZ(constraintTarget.altReference.position), 2);
            //BugLine.Instance.markPoint(new VectorXZ(constraintTarget.reference.position), 3);
            BugLine.Instance.circle(center, pole.magnitude); // constraintTarget.reference.position, pole.magnitude);
        }

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
        return constraintTarget.lineSegmentReference.closestPoint(p);
    }

    private static bool SameSign(float a, float b) {
        return NearlyEqual(Mathf.Sign(a), Mathf.Sign(b));
    }

    private static bool NearlyEqual(float a, float b) {
        if (a == b) { return true; }
        return Mathf.Abs(a - b) < Mathf.Epsilon;
    }

}
