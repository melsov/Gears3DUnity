using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Assertions;
using System.Collections.Generic;
using UnityEditor;

//TODO: connectTo method will apply for any free-rotatable drivable
// make class FreeRotatableDrivabe
[System.Serializable]
public class Pole : Drivable
{
    protected override void awake() {
        base.awake();
        print("pole awake: y is: " + transform.position.y);
        print("pegboard null ? " + (_pegboard == null) + "\n back sock set null: " + (_pegboard.getBackendSocketSet() == null));
        Assert.IsTrue(_pegboard.getBackendSocketSet().sockets.Length == 2);
        Assert.IsTrue(_pegboard.getFrontendSocketSet().sockets.Length == 2);
    }

    protected override bool vConnectTo(Collider other) {
        if (isConnectedTo(other.transform)) { return false; }
        Socket aSocket;
        Peg peg = _pegboard.getBackendSocketSet().closestOpenPegOnFrontendOf(other, out aSocket);
        if (peg != null) {
            if (RotationModeHelper.CompatibleModes(peg.pegIsParentRotationMode, aSocket.socketIsChildRotationMode)) {
                debugY();
                setSocketToPeg(aSocket, peg);
                debugY(); //
                return true;
            }
        }
        
        return false;
    }

//TODO: poles teleport up (in y) when they connect to gears: codify what y everything should be on

    // POLE CONNECT CASES:
    //  A: NEITHER BACKEND CONNECTED: try to set a backend socket
    //  B: ONE BACKEND CONNECTED: try to set a frontend socket

//CONSIDER: make a drivable default version of vMakeConn...
    protected override bool vMakeConnectionWithAfterCursorOverride(Collider other) {
        if (isConnectedTo(other.transform)) {
            return false;
        }
        // CASE 1: this is a socket that has a hinge connection such that 
        // we can connect to it and still keep any parent connection? 
        // CASE A:
        if (! _pegboard.getBackendSocketSet().isConnected()) {
            return vConnectTo(other);
        }

        //CASE B:
        print("vMake connection after cursor override: other name: " + other.name);
        Collider dOC = GetComponent<CursorAgent>().dragOverrideCollider;
        Handle handle = dOC.GetComponent<Handle>();
        Socket frontSocket = handle.widget.GetComponent<Socket>();
        Assert.IsTrue(frontSocket != null);

        Peg aPeg = null;
        Socket aSocket = null;
        ISocketSetContainer ssc = other.GetComponentInChildren<ISocketSetContainer>(); // TransformUtil.FindComponentInThisOrChildren<ISocketSetContainer>(other.transform); 
        if (ssc == null) {
            print("no socket set on");
            Bug.printComponents(other.gameObject);
            return false;
        }
        SocketSet otherBackendSet = ssc.getBackendSocketSet();
        if (otherBackendSet == null) {
            print("other backend socket set null");
            return false;
        }
        if (frontSocket.hasChildPeg()) {
            print("front soc has child peg");
            aPeg = frontSocket.childPeg;
            // get an open backend socket on other
            aSocket = otherBackendSet.getOpenChildSocketClosestTo(aPeg.transform.position, aPeg.pegIsParentRotationMode);
            if (aSocket != null) {
                print("open child socket?");
                setSocketToPeg(aSocket, aPeg);
                return true;
            }
        } else {
            if (connectToBackendOf(otherBackendSet, frontSocket)) {
                return true;
            }
        }
        // is a constraint applied where available?
        return false;
    }

    private bool connectToBackendOf(SocketSet otherBackendSet, Socket frontSocket) {
        Socket aSocket = frontSocket;

        // get a peg on backend of other
        Socket childSocket = otherBackendSet.getChildSocketWithParentPegClosestTo(aSocket.transform.position, RotationMode.FREE_OR_FIXED); //CONSIDER: do we care about ro mode?
        if (childSocket != null) {
            if (childSocket.drivingPeg != null) {
                childSocket.drivingPeg.beChildOf(aSocket);
                return true;
            }
        }
        return false;
    }

    public bool acceptBackendPegOnDrivable(Drivable d) {
        Socket frontSocket = _pegboard.getFrontendSocketSet().getOpenParentSocketClosestTo(d.transform.position, RotationMode.FREE_OR_FIXED);

        // if this front socket is 'on top of' a driven back end socket use the opposite frontend socket instead
        Socket backSocket = _pegboard.closestOppositeEndSocket(frontSocket);
        if (backSocket != null && backSocket.hasDrivingPeg()) {
            Socket another = _pegboard.getFrontendSocketSet().getAnother(frontSocket);
            if (another != null) {
                frontSocket = another;
            }
        }
        //DBUG
        BugLine.Instance.markPoint(new VectorXZ(frontSocket.transform.position), 1); //DBUG
        EditorApplication.isPaused = true;

        if (frontSocket == null) { return false; }
        ISocketSetContainer ssc = d.GetComponentInChildren<ISocketSetContainer>();
        if (ssc != null) {
            return connectToBackendOf(ssc.getBackendSocketSet(), frontSocket);
        }
        return false;
    }

    //CONSIDER: what kinds of parent constraints should go with what kinds of child constraints and where to enforce these decisions
    public override Constraint parentConstraintFor(Constraint childConstraint, Transform childTransform) {
        Socket socket = childConstraint.constraintTarget.target.GetComponent<Socket>();

        if (!_pegboard.getFrontendSocketSet().contains(socket)) { // this would be queer
            return null;
        }
        Socket freeRotatingBackendSocket = null;
        foreach(Socket s in _pegboard.getBackendSocketSet().sockets) {
            if (s.isFreeRotatingOnPeg()) {
                freeRotatingBackendSocket = s;
                break;
            }
        }
        if (freeRotatingBackendSocket == null) {
            return null;
        }
        // set up 'linear actuator constraint'
        LinearActuatorConstraint laConstraint = GetComponent<LinearActuatorConstraint>();
        if (laConstraint == null) {
            laConstraint = gameObject.AddComponent<LinearActuatorConstraint>();
        }
        LineSegment ls = null;
        if (childTransform.GetComponentInParent<LinearActuator>() != null) {
            ls = childTransform.GetComponentInParent<LinearActuator>().GetComponentInChildren<LineSegment>();
            laConstraint.constraintTarget.lineSegmentReference = ls;
        }
        laConstraint.constraintTarget.target = childTransform;
        laConstraint.constraintTarget.reference = freeRotatingBackendSocket.transform;
        laConstraint.constraintTarget.altReference = socket.transform;
        laConstraint.isParentConstraint = true;

        Peg peg = childTransform.GetComponent<Peg>();
        if (peg != null) {
            Drivable drChild = peg.GetComponentInParent<Drivable>();
            Drivable gear = freeRotatingBackendSocket.drivingPeg.parent.GetComponentInParent<Drivable>();
            
            laConstraint.configure(gear, drChild);
        }
        return laConstraint;
    }

    //TODO: pole and in general drivable needs to lose parent constraint on disconnect
    //TODO: give constraint a chance to adjust on move of either drivable

    protected override void vDisconnect() {
        base.vDisconnect();
        _pegboard.getBackendSocketSet().removeConstraintTargetSets();
        _pegboard.getFrontendSocketSet().removeConstraintTargetSets();
    }

    public override float driveScalar() {
        return 0;
    }

    public override Drive receiveDrive(Drive drive) {
        return drive;
    }

    public override void restoreConnectionData(ref List<byte[]> connectionData) {
        print("%%%welcome to pole: restore connection data");
        base.restoreConnectionData(ref connectionData);
    }
}
