using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class Exchanger : MonoBehaviour {

    [SerializeField]
    protected bool impartsRotation = true;
    private delegate void RotateExchangable(Quaternion q);
    private RotateExchangable rotateExchangable = delegate (Quaternion q) { };
    
    [SerializeField]
    protected Transform keep;
    private bool isAnimatingRealign;

    protected Rigidbody rb;

    public void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    public virtual bool hasExchangable {
        get { return exchangable; }
    }

    protected Exchangable exchangable {
        get { return keep.GetComponentInChildren<Exchangable>(); }
    }

    public virtual bool canGive() {
        return hasExchangable;
    }

    public virtual bool canAccept(Exchangable ex) {
        if (hasExchangable) { return false; }

        return true;
    }

    public bool available { get { return !hasExchangable && !isAnimatingRealign; } }

    public bool give(Exchanger other) {
        if (isAnimatingRealign || isDownTime) { return false; }
        other.accept(exchangable);
        //coroutine to check having truly escaped from other Excgr
        StartCoroutine(checkClearOf(other));
        return true;
    }

    #region check-excape
    private bool checking;
    private List<Exchanger> ignore = new List<Exchanger>();
    private IEnumerator checkClearOf(Exchanger other) {
        if(!checking || ignore.Count == 0) {
            checking = true;
            if (!ignore.Contains(other)) {
                ignore.Add(other);
            }
            while(ignore.Count > 0) {
                yield return new WaitForSeconds(Time.maximumDeltaTime);
                Exchanger ex = ignore[0];
                if(outOfRange(ex._collider) && (!ex.hasExchangable || outOfRange(ex.exchangable.GetComponent<Collider>()))) {
                    ignore.RemoveAt(0);
                    print("rem");
                }
            }
            checking = false;
        }
    }

    private Collider _collider { get { return GetComponent<Collider>(); } }

    private bool outOfRange(Collider collider) {
        if (!collider) { return true; }
        foreach(Collider c in Physics.OverlapBox(_collider.bounds.center,_collider.bounds.extents * 1.5f,collider.transform.rotation)) {
            if(collider == c) { return false; }
        }
        return true;
    }

    #endregion

    private Exchangable getExchangable(Collider other) { return other.GetComponent<Exchangable>(); }
    private Exchanger findExchanger(Collider other) { return other.GetComponent<Exchanger>(); }

    public virtual void OnTriggerEnter(Collider other) {
        //Exchangable ex = getExchangable(other);
        //if(!ex) { return; }
        //pickupFreeAgent(ex);
    }

    public virtual void OnTriggerStay(Collider other) {
        if (!available) { return; }
        Exchangable ex = getExchangable(other);
        if(!ex) { return; }
        //if (pickupFreeAgent(ex)) { return; }

        if (!ex.owner) { return; } //if no owner, we really don't know what you're doing here.
        if (ignore.Contains(ex.owner)) { return; }
        //if we don't already own ex
        //and ex has an owner
        VectorXZ toKeep = keep.position - ex.transform.position;
        VectorXZ relVel = ex.velocity - vel;

        if (toKeep.dot(relVel) < 0f) {
            Exchanger former = ex.owner;
            if (ex.owner.give(this)) {
                haveDownTime();
                former.haveDownTime();
            }
            
        }
        //keep track of ex's entry point. ex must cross over a line (probably Vec3.right from keep)
        //being able to use velocity in this case would probably be convenient. (insist on exchangables having rbs?)
        
    }

    private void haveDownTime() {
        StartCoroutine(doDownTime());
    }

    private IEnumerator doDownTime() {
        isDownTime = true;
        yield return new WaitForSeconds(.6f);
        isDownTime = false;
    }

    private bool isDownTime;
    private Vector3 prevPos;
    private Vector3 currentPos;

    private Vector3 vel { get { return currentPos - prevPos; } }

    public void FixedUpdate() {
        prevPos = currentPos;
        currentPos = rb.position;
    }

    private bool pickupFreeAgent(Exchangable ex) {
        if(!ex.owner && !ex.unavailable) {
            return accept(ex);
        }
        return false;
    }

    public bool accept(Exchangable _exchangable) {
        if (!canAccept(_exchangable)) { return false; }

        _exchangable.transform.parent = keep;
        setupRotateExchangable();
        StartCoroutine(animatedRealign(_exchangable));
        return true;        
    }

    private void setupRotateExchangable() {
        if(!exchangable) { return; }
        if (!impartsRotation) {
            rotateExchangable = delegate (Quaternion q) { };
            return;
        }
        rotateExchangable = delegate (Quaternion q) {
            exchangable.rotate(q);
        };
    }

    private IEnumerator animatedRealign(Exchangable exchangable_) {
        isAnimatingRealign = true;
        Quaternion q;
        Vector3 v;
        float frames = 24;
        Vector3 destination = TransformUtil.GetAlignedXZ(exchangable_.transform, keep, exchangable_.alignTarget);
        for(int i = 0; i < frames; ++i) {
            yield return new WaitForFixedUpdate();
            q = Quaternion.Slerp(exchangable_.transform.rotation, keep.rotation, i / frames);
            v = Vector3.Lerp(exchangable_.transform.position, destination, i / frames);
            exchangable_.move(v);
            //exchangable_.rotate(q);
        }
        isAnimatingRealign = false;
    }

    public void realign(Exchangable _exchangable) {
        if (_exchangable != exchangable) { return; }
        StartCoroutine(animatedRealign(_exchangable));
    }

    public void disown(Exchangable _exchangable) {
        if (_exchangable != exchangable) { return; }
        _exchangable.transform.SetParent(null);
    }

}
