using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PolarizableTube : OneWayTube {

    public Transform debugArrow;

    protected override void awake() {
        base.awake();
        TubeAccess[] tas = GetComponentsInChildren<TubeAccess>();
        UnityEngine.Assertions.Assert.IsTrue(tas.Length == 2, "Need exactly two Tube Access Points");
        accessPointPair = new AccessPointPair(tas[0], tas[1]);
        setDebugArrow();
    }

    protected override Transform entrance {
        get {
            return accessPointPair.entrance.transform;
        }
        set { }
    }

    protected override Transform exit {
        get {
            return accessPointPair.exit.transform;
        }
        set { }
    }

    private Vector3 flow { get { return exit.position - entrance.position; } }

    private void setDebugArrow() {
        Quaternion ro = Quaternion.LookRotation(flow);
        debugArrow.rotation = ro;
    }

    private AccessPointPair accessPointPair;
    private HashSet<Transform> occupants = new HashSet<Transform>();

    private void addOccupant(Transform t) {
        occupants.Add(t);
    }
    private void removeOccupant(Transform t) {
        occupants.Remove(t);
    }
    private bool occupied { get { return occupants.Count > 0; } }

    private bool isDispensable(Collider collider) { return collider.GetComponentInParent<Dispensable>(); }

    public void accessPointTriggerEnter(TubeAccess tubeAccess, Collider collider) {
        if (!isDispensable(collider)) { return; }
        if (!accessPointPair.setEntrance(tubeAccess)) {
            rejectCollider(tubeAccess, collider);
            return;
        }
        addOccupant(collider.transform);
    }

    public void accessPointTriggerExit(TubeAccess tubeAccess, Collider collider) {
        if (!isDispensable(collider)) { return; }
        removeOccupant(collider.transform);
        if (!occupied) {
            accessPointPair.setUndetermined();
        }
    }

    public override void OnTriggerStay(Collider other) {
        if (accessPointPair.undetermined) { return; }
        setDebugArrow();
        OnTriggerEnter(other);
    }

    private void rejectCollider(TubeAccess tubeAccess, Collider collider) {
        if (collider.GetComponent<Rigidbody>()) {
            VectorXZ dif = collider.transform.position - tubeAccess.transform.position;
            collider.GetComponent<Rigidbody>().AddForce(dif.normalized.vector3() * 200, ForceMode.VelocityChange);
        }
    }

    protected class AccessPointPair
    {
        TubeAccess a, b;
        public AccessPointPair(TubeAccess a, TubeAccess b) {
            this.a = a;
            this.b = b;
        }

        public TubeAccess entrance {
            get {
                if (a.isEntrance) { return a; }
                return b;
            }
        }
        public TubeAccess exit {
            get {
                if (b.isExit) { return b; }
                return a;
            }
        }
        
        public bool undetermined { get { return a.undetermined && b.undetermined; } }
        private TubeAccess getOther(TubeAccess tubeAccess) {
            if (tubeAccess == a) { return b; }
            else if (tubeAccess == b) { return a; }
            throw new System.Exception("AccessPointPair using an access point that it doesn't own?");
        }

        public bool setEntrance(TubeAccess tubeAccess) {
            if (undetermined || getOther(tubeAccess).accessPointStatus == AccessPointStatus.EXIT) {
                setBoth(tubeAccess);
                return true;
            }
            return false;
        }

        private void setBoth(TubeAccess entrance) {
            entrance.accessPointStatus = AccessPointStatus.ENTRANCE;
            getOther(entrance).accessPointStatus = AccessPointStatus.EXIT;
        }

        public void setUndetermined() {
            a.accessPointStatus = AccessPointStatus.UNDETERMINED;
            b.accessPointStatus = AccessPointStatus.UNDETERMINED;
        }

        public void dbug() {
            if (undetermined) { print("undetermind"); }
            else if (entrance == a) { print("a is entrance;"); }
            else { print("b is entrance"); }
        }
    }
}


