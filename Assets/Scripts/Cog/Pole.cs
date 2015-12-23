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

    public override float driveScalar() {
        return 0;
    }

    public override Drive receiveDrive(Drive drive) {
        return drive;
    }
}
