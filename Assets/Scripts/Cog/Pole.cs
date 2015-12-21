using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Assertions;

public class Pole : Drivable
{
    protected override void awake() {
        base.awake();
        Assert.IsTrue(getBackendSocketSet().sockets.Length == 2);
    }

    protected override bool vConnectTo(Collider other) {
        Socket aSocket;
        Peg peg = closestOpenPegOn(other, out aSocket);
        if (peg != null) {
            //if peg is parent rotation mode is fixed, parent pole to peg
            if (true) { // if peg parent ro mode fixed
                setSocketToPeg(aSocket, peg);
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
