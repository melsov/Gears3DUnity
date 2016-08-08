using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TubeAccess : MonoBehaviour
{
    private WeakReference _polarizableTube;
    protected PolarizableTube polarizableTube {
        get { return (PolarizableTube)_polarizableTube.Target; }
    }

    public AccessPointStatus accessPointStatus = AccessPointStatus.UNDETERMINED;
    public bool undetermined { get { return accessPointStatus == AccessPointStatus.UNDETERMINED; } }
    public bool isEntrance { get { return accessPointStatus == AccessPointStatus.ENTRANCE; } }
    public bool isExit { get { return accessPointStatus == AccessPointStatus.EXIT; } }

    public void Awake() {
        PolarizableTube tb = GetComponentInParent<PolarizableTube>();
        UnityEngine.Assertions.Assert.IsTrue(tb, "need a polarizable tube parent or else this isn't going to work.");
        _polarizableTube = new WeakReference(tb);
    }

    public void OnTriggerEnter(Collider other) {
        polarizableTube.accessPointTriggerEnter(this, other);
    }

    public void OnTriggerExit(Collider other) {
        polarizableTube.accessPointTriggerExit(this, other);
    }
}

public enum AccessPointStatus
{
    UNDETERMINED, ENTRANCE, EXIT
};
