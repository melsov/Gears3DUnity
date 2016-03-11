using UnityEngine;
using System.Collections.Generic;
using System;

public class Rope : Drivable {

    public ChainLink linkPrefab;
    public Rigidbody baseLink;
    public int linkCount;
    public float xOffset = .1f;
    protected List<ChainLink> links;

    protected override void awake() {
        base.awake();
        linkPrefab.gameObject.SetActive(false);
        construct();
    }

    private void construct() {
        Rigidbody lastRb = baseLink;
        for (int i = 0; i < linkCount; ++i) {
            ChainLink cl = Instantiate<ChainLink>(linkPrefab);
            cl.gameObject.SetActive(true);
            cl.transform.position = new Vector3(lastRb.transform.position.x, lastRb.transform.position.y, lastRb.transform.position.z - (cl.length + xOffset));
            cl.connectedRigidbody = lastRb;
            lastRb = cl.rigidbod;
        }
    }

    public override float driveScalar() {
        return 0f;
    }

    public override Drive receiveDrive(Drive drive) {
        return drive;
    }

}
