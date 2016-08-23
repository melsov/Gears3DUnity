using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

//TODO: if you are owned, let exchangers handle the exchange
//always let them handle picking you up.
//if you're being dragged, you're off. Exchangers use onTriggerStay

public class Exchangable : MonoBehaviour {

    private HashSet<Exchanger> blocked = new HashSet<Exchanger>();

    [SerializeField]
    protected Transform _alignTarget;
    public Transform alignTarget {
        get {
            if (!_alignTarget) { _alignTarget = transform; }
            return _alignTarget;
        }
    }

    private Cog cog;
    private Rigidbody rb;

    private delegate void RotateExchangable(Quaternion q);
    private RotateExchangable rotateExchangable = delegate (Quaternion q) { };

    private delegate void MoveEx(Vector3 v);
    private MoveEx moveEx;

    public void rotate(Quaternion q) {
        rotateExchangable(q);
    }

    public void move(Vector3 v) {
        moveEx(v);
    }

    private void setupRotateMoveExchangable() {
        if (cog) {
            rotateExchangable = delegate (Quaternion q) {
                cog.rotate(q);
            };
            moveEx = delegate (Vector3 v) { cog.move(v); };
        } else if (rb) {
            rotateExchangable = delegate (Quaternion q) {
                rb.MoveRotation(q);
            };
            moveEx = delegate (Vector3 v) { rb.MovePosition(v); };
        } else {
            rotateExchangable = delegate (Quaternion q) {
                transform.rotation = q;
            };
            moveEx = delegate (Vector3 v) { transform.position = v; };
        }
    }

    private Collider _collider { get { return GetComponent<Collider>(); } }
    
    public Exchanger owner {
        get { return GetComponentInParent<Exchanger>(); }
    }

    public delegate void OnExchanged(Exchangable exchangable);
    private OnExchanged onExchanged = delegate (Exchangable e) { };

    public void register(OnExchanged onExchanged) {
        this.onExchanged += onExchanged;
    }
    public void unregister(OnExchanged onExchanged) {
        this.onExchanged -= onExchanged;
    }

    private bool ready = true;
    public bool closedDown {
        get;
        set;
    }

    public void Awake() {
        cog = GetComponent<Cog>();
        rb = GetComponent<Rigidbody>();
        setupRotateMoveExchangable();
    }

    public Vector3 velocity { get { return curPos - prevPos; } }

    private Vector3 prevPos;
    private Vector3 curPos;
    public void FixedUpdate() {
        prevPos = curPos;
        curPos = rb.position;
    }

    private bool checkEscapedOwner(Collider[] colliders) {
        if (owner) {
            foreach (Collider c in colliders) {
                if(c.GetComponent<Exchanger>() == owner) {
                    owner.realign(this);
                    return false;
                }
            }
            owner.disown(this);
        }
        return true;
    }

    public void detectExchangers() {
        Collider[] colliders = Physics.OverlapBox(_collider.bounds.center, _collider.bounds.extents, _collider.transform.rotation);
        if (!checkEscapedOwner(colliders)) {
            return;
        }
        foreach(Collider c in colliders) {
            if (handleCollider(c)) {
                return;
            }
        }
    }

    public bool unavailable { get { return !ready || closedDown; } }



    protected virtual bool handleCollider(Collider other) {
        print("1");
        if(unavailable) { return false; }
        print("2");
        Exchanger exchanger = other.GetComponentInParent<Exchanger>();
        if (!exchanger) { return false; }
        print("3");
        if (blocked.Contains(exchanger)) { return false; }
        if (exchanger.accept(this)) {
            //block(exchanger);
            StartCoroutine(closeDownForAMoment());
            onExchanged(this);
            return true;
        }
        return false;
    }

    private static bool ColliderOverlaps(Collider collider, Collider other) {
        foreach(Collider c in Physics.OverlapBox(collider.bounds.center, collider.bounds.extents, collider.transform.rotation)) {
            if (c == other) {
                return true;
            }
        }
        return false;
    }

    //private IEnumerator unblockCollider(Collider other) {
    //    if(blocked.Contains(other.GetComponent<Exchanger>())) {
    //        yield return new WaitForSeconds(.5f);
    //        while(blocked.Contains(other.GetComponent<Exchanger>())) {
    //            yield return new WaitForSeconds(Time.maximumDeltaTime);
    //            if(!ColliderOverlaps(_collider, other)) {
    //                blocked.Remove(other.GetComponent<Exchanger>());
    //            }
    //        }
    //    }
    //}

    public virtual void OnTriggerExit(Collider other) {
        //unblockCollider(other);
        //unblock(other.GetComponent<Exchanger>());
    }

    private IEnumerator closeDownForAMoment() {
        ready = false;
        yield return new WaitForSeconds(.4f);
        ready = true;
    }

    //private void block(Exchanger ex) {
    //    //if(!ex) { return; }
    //    //blocked.Add(ex);
    //}

    //private void unblock(Exchanger ex) {
    //    //if(!ex) { return; }
    //    //blocked.Remove(ex);
    //}

    
}
