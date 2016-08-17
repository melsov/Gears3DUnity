using UnityEngine;
using System.Collections;



public interface IObservableFloatProvider
{
    ObservableFloat getObservableFloat();
}

public class Scale : Switch , IObservableFloatProvider {

    protected ObservableFloat _omass;
    protected ObservableFloat omass {
        get {
            if (_omass == null) {
                _omass = new ObservableFloat();
            }
            return _omass;
        }
    }
    private float lastMass;
    [SerializeField]
    private float threshhold = 4f;

    private Spring sj;
    private Rigidbody platformRB;
    private float startPosition;
    private delegate float GetAFloat();
    private GetAFloat getPosition;
    private GetAFloat getGravity;

    private Arc arc;

    [SerializeField]
    protected float maxWeight = 50f;

    [SerializeField]
    protected Transform threshholdIndicator;

    protected NeedleDisplay needleDisplay;

    protected override void awake() {
        base.awake();
        sj = GetComponentInChildren<Spring>();
        platformRB = sj.GetComponent<Rigidbody>();
        getPosition = delegate () { return platformRB.transform.localPosition.z - sj.connectedBody.transform.localPosition.z; };
        startPosition = getPosition();
        getGravity = delegate () { return Physics.gravity.z; };
        arc = GetComponentInChildren<Arc>();
        needleDisplay = GetComponentInChildren<NeedleDisplay>();
        needleDisplay.max = maxWeight;
    }

    public ObservableFloat getObservableFloat() {
        return omass;
    }

    private float displacement {
        get { return getPosition() - startPosition; }
    }

    private float springForce {
        get { return sj.spring * displacement + sj.damper * platformRB.velocity.z; }
    }

    private float mass {
        get { return springForce / getGravity() - platformRB.mass; }
    }

    public void FixedUpdate() {
        omass.Value = mass;
        checkToggle();
        lastMass = mass;
    }

    private void checkToggle() {
        if (lastMass < threshhold == mass > threshhold) {
            toggle();
        }
    }

    protected override void toggle() {
        SwitchState state = mass - lastMass > 0f ? SwitchState.ON : SwitchState.OFF;
        on.setState(state);
        updateIndicator();
        updateClient();
    }

    protected override void vStartDragOverride(CursorInfo ci) {
    }

    protected override void vDragOverride(CursorInfo ci) {
        float gradient = arc.gradient(ci.current.vector3());
        threshholdIndicator.rotation = arc.between(gradient);
        threshhold = gradient * maxWeight;
    }

    protected override void vEndDragOverride(CursorInfo ci) {
        base.vEndDragOverride(ci);
    }
}
