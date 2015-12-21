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
        Peg peg = backendSocketSet.closestOpenPegOnFrontendOf(other, out aSocket);
        if (peg != null) {
            //if peg parent rotation mode is fixed or 'free or fixed'

            //CONSIDER: POLE CASES:
            // FIXED ONLY (connected to an axel )
            // FREE 
            // in all cases: only one parent but poles can be pushed around possibly
            // also: make poles mouse rotatable
            if (RotationModeHelper.CompatibleModes(peg.pegIsParentRotationMode, RotationMode.FIXED_ONLY)) { 
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
