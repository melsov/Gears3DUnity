using UnityEngine;
using System.Collections;
using System;

public class SelfContainedTetheredPair : SwitchExtension
{
    [SerializeField]
    protected Rigidbody satellite;
    private Vector3 satelliteHoldPosition;
    private Vector3 baseHoldPosition;
    private DragHistory dragOverrideHistory;

    protected override void awake() {
        base.awake();
    }
    public override void Start() {
        base.Start();
        satelliteHoldPosition = satellite.position;
    }

    public override ContractPortfolio.ClientTree.Node node {
        get {
            if (_node == null) { 
                /* Node that doesn't propagate moves to its children */
                _node = new ContractPortfolio.ClientTree.Node(contractPortfolio, false);
            }
            return _node;
        }
    }

    protected override void setupMoveAndRotate() {
        base.setupMoveAndRotate();
        move = delegate (Vector3 global) {
            rb.MovePosition(global);
            satellite.Sleep();
            //satellite.AddForce(TransformUtil.forceForTargetPosition(satellite, satelliteHoldPosition)); 
        };
    }

    #region logic-gate

    protected override int maxClients {
        get {
            return 1;
        }
    }

    protected override int maxProducers {
        get {
            return 1;
        }
    }

    protected ISwitchStateProvider producer {
        get {
            foreach (ISwitchStateProvider s in producerSSPs()) {
                return s;
            }
            return null;
        }
    }

    protected override SwitchState calculateState(ISwitchStateProvider ignore) {
        if (producer == null || producer == ignore) { return SwitchState.OFF; }
        return producer.currentState();
    }

    #endregion


    protected override void vStartDragOverride(CursorInfo ci) {
        //rbToStubbornNonKine();
        dragOverrideHistory = new DragHistory(satellite.position, ci.current);
        normalDragStart(ci.current);
    }

    protected override void vDragOverride(CursorInfo ci) {
        Vector3 relative = dragOverrideHistory.updateLastCursorGetRelative(satellite.transform.position, ci.current).vector3();
        satellite.MovePosition(satellite.transform.position + relative);
        clientTree.moveChildrenRelative(relative);
        satelliteHoldPosition = satellite.transform.position;
    }

    //Want empty
    protected override void vEndDragOverride(CursorInfo ci) {
        satelliteHoldPosition = satellite.transform.position;
    }

}

public class RevertableRigidbody
{
    public readonly Rigidbody rb;
    private readonly RBState rbState;

    public struct RBState
    {
        public readonly float mass;
        public readonly float drag;
        public readonly float angularDrag;
        public readonly bool useGravity;
        public readonly bool isKinematic;

        public RBState(Rigidbody r) {
            mass = r.mass;
            drag = r.drag;
            angularDrag = r.angularDrag;
            useGravity = r.useGravity;
            isKinematic = r.isKinematic;
        }

        public RBState(float mass, float drag, float angularDrag, bool useGravity, bool isKinematic) {
            this.mass = mass; this.drag = drag; this.angularDrag = angularDrag; this.useGravity = useGravity; this.isKinematic = isKinematic;
        }

        public void set(Rigidbody r) {
            r.mass = mass;
            r.drag = drag;
            r.angularDrag = angularDrag;
            r.useGravity = useGravity;
            r.isKinematic = isKinematic;
        }
    }

    public RevertableRigidbody(Rigidbody rb) {
        this.rb = rb;
        rbState = new RBState(rb);
    }

    public void revert() { rbState.set(rb); }

    public void convert(RBState rbState) { rbState.set(rb); }

    public static implicit operator Rigidbody(RevertableRigidbody rr) { return rr.rb; }
}
