using UnityEngine;
using System.Collections;

public class OpenCloseAnimationHandler : MonoBehaviour {

    protected Animator animator;
    [SerializeField]
    protected string openParameterName = "Opening";
    public delegate void StateChangedTo(bool isOpen);
    public StateChangedTo stateChangedTo;

    public void Awake() {
        animator = GetComponent<Animator>();
        AnimatorStateInfo asi = animator.GetCurrentAnimatorStateInfo(0);
        AnimatorClipInfo[] acis = animator.GetCurrentAnimatorClipInfo(0);
        print(acis.Length);
        foreach(AnimatorClipInfo aci in acis) {
            print(aci.clip.wrapMode.ToString());
        }
        AnimatorClipInfo[] nacis = animator.GetNextAnimatorClipInfo(0);
        print(nacis.Length);
        foreach(AnimatorClipInfo aci in nacis) {
            print(aci.clip.name);
            print(aci.clip.wrapMode.ToString());
        }
    }
    

    public void open(bool _open) {
        animator.SetInteger(openParameterName, _open ? 1 : -1);
    }

    public void open() {
        open(true);
    }

    public void close() {
        open(false);
        setWrapModeForCurrentClip(WrapMode.ClampForever);
    }

    public void stayClosed() {
        stateChangedTo(false);
        //setWrapModeForCurrentClip(WrapMode.ClampForever);
        //animator.SetInteger(openParameterName, 0);
    }
    protected AnimationClip currentClip {
        get { return animator.GetCurrentAnimatorClipInfo(0)[0].clip; }
    }
    protected AnimatorStateInfo currentStateInfo {
        get {
            return animator.GetCurrentAnimatorStateInfo(0);
        }
    }
    protected void setWrapModeForCurrentClip(WrapMode wm) {
        currentClip.wrapMode = wm;
    }
    
    public void stayOpen() {
        print("stay open");
        stateChangedTo(true);
        //setWrapModeForCurrentClip(WrapMode.ClampForever);
        //animator.SetInteger(openParameterName, 2);
    }

}


