using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Assertions;

//TODO: connectTo method will apply for any free-rotatable drivable
// make class FreeRotatableDrivabe
public class Pole : Drivable
{
    protected override void awake() {
        base.awake();
        Assert.IsTrue(_pegboard.getBackendSocketSet().sockets.Length == 2);
        Assert.IsTrue(_pegboard.getFrontendSocketSet().sockets.Length == 2);
    }

    protected override bool vConnectTo(Collider other) {
        Socket aSocket;
        Peg peg = _pegboard.getBackendSocketSet().closestOpenPegOnFrontendOf(other, out aSocket);
        if (peg != null) {
            if (RotationModeHelper.CompatibleModes(peg.pegIsParentRotationMode, aSocket.socketIsChildRotationMode)) {
                setSocketToPeg(aSocket, peg);
                return true;
            }
        }
        return false;
    }

    // POLE CONNECT CASES:
    //  A: NEITHER BACKEND CONNECTED: try to set a backend socket
    //  B: ONE BACKEND CONNECTED: try to set a frontend socket

//CONSIDER: make a drivable default version of vMakeConn...
    protected override bool vMakeConnectionWithAfterCursorOverride(Collider other) {
        // CASE 1: this is a socket that has a hinge connection such that 
        // we can connect to it and still keep any parent connection? 
        // CASE A:
        if (! _pegboard.getBackendSocketSet().isConnected()) {
            return vConnectTo(other);
        }

        //CASE B:
        print("vMake connection");
        Collider dOC = GetComponent<CursorAgent>().dragOverrideCollider;
        Handle handle = dOC.GetComponent<Handle>();
        Socket frontSocket = handle.widget.GetComponent<Socket>();
        Assert.IsTrue(frontSocket != null);

        Peg aPeg = null;
        Socket aSocket = null;
        ISocketSetContainer ssc = TransformUtil.FindComponentInThisOrChildren<ISocketSetContainer>(other.transform); // other.GetComponent<ISocketSetContainer>();
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
            print("front soc no child peg");
            aSocket = frontSocket;
            // get a peg on backend of other
            Socket childSocket = otherBackendSet.getChildSocketWithParentPegClosestTo(aSocket.transform.position, RotationMode.FREE_OR_FIXED); //CONSIDER: do we care about ro mode?
            if (childSocket != null) {
                if (childSocket.drivingPeg != null) {
                    childSocket.drivingPeg.beChildOf(aSocket);
                    return true;
                }
            }
        }
        // is a constraint applied where available?
        return false;
    }

    public override Constraint parentConstraintFor(Constraint childConstraint, Transform childTransform) {
        Socket socket = childConstraint.constraintTarget.target.GetComponent<Socket>();

        if (!_pegboard.getFrontendSocketSet().contains(socket)) { // this would be queer
            return null;
        }
        Socket freeRotatingBackendSocket = null;
        foreach(Socket s in _pegboard.getBackendSocketSet().sockets) {
            print("s has parent: " + s.hasDrivingPeg());
            if (s.isFreeRotatingOnPeg()) {
                freeRotatingBackendSocket = s;
                break;
            }
        }
        if (freeRotatingBackendSocket == null) {
            print("opposite soc is null");
            return null;
        }
        // set up 'look at constraint'
        print("set up look at constraint");
        LookAtConstraint lookAtConstraint = gameObject.AddComponent<LookAtConstraint>();
        lookAtConstraint.constraintTarget.target = childTransform;
        lookAtConstraint.constraintTarget.reference = freeRotatingBackendSocket.transform;
        lookAtConstraint.constraintTarget.altReference = socket.transform;
        lookAtConstraint.isParentConstraint = true;
        return lookAtConstraint;
    }

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
}
