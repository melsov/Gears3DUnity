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
        Assert.IsTrue(getBackendSocketSet().sockets.Length == 2);
        Assert.IsTrue(getFrontendSocketSet().sockets.Length == 2);
    }

    protected override bool vConnectTo(Collider other) {
        Socket aSocket;
        Peg peg = backendSocketSet.closestOpenPegOnFrontendOf(other, out aSocket);
        if (peg != null) {
            if (RotationModeHelper.CompatibleModes(peg.pegIsParentRotationMode, aSocket.socketIsChildRotationMode)) {
                setSocketToPeg(aSocket, peg);
                return true;
            }
        }
        return false;
    }

//CONSIDER: make this the drivable default method
    protected override bool vMakeConnectionWithAfterCursorOverride(Collider other) {
        // CASE 1: this is a socket that has a hinge connection such that 
        // we can connect to it and still keep any parent connection? 
        
        // do we have a front end socket with a peg (that's unoccupied) and
        // is there a backend socket on other that we could connect this peg to?
        Peg openFrontEndPeg = null;
        Socket backendSocket = getFrontendSocketSet().openBackendSocketOnOtherClosestToOpenPegOnThis(other.transform, out openFrontEndPeg);
        if (backendSocket != null) {
            if (RotationModeHelper.CompatibleModes(openFrontEndPeg.pegIsParentRotationMode, backendSocket.socketIsChildRotationMode)) {
                setSocketToPeg(backendSocket, openFrontEndPeg);
                // TODO: when making second connections. use fixed joints
            }
        }
        return false;
    }

    public override float driveScalar() {
        return 0;
    }

    public override Drive receiveDrive(Drive drive) {
        return drive;
    }
}
