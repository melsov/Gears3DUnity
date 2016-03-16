using UnityEngine;
using System.Collections.Generic;
using System;

public class Rope : Drivable {

    //TO NOT DO: guide rope around gears and pegs
    //TODO: add pegboards to ends of ropes
    //TODO: create "Combiner": box with two or more inputs and an output. 
    //When dispensibles coinhabit the combiner, they fuse into a new dispensible. Dispensibles go through the combiner quickly either way.

    public ChainLink linkPrefab;
    public Rigidbody baseLink;
    public int linkCount;
    public float xOffset = .05f;
    protected List<ChainLink> links;

    protected override void awake() {
        base.awake();
        linkPrefab.gameObject.SetActive(false);
        links = new List<ChainLink>();
        construct();
    }

    private void construct() {
        links.RemoveRange(0, links.Count);
        Rigidbody lastRb = baseLink;
        for (int i = 0; i < linkCount; ++i) {
            ChainLink cl = Instantiate<ChainLink>(linkPrefab);
            cl.rope = this;
            cl.gameObject.SetActive(true);
            cl.transform.position = new Vector3(lastRb.transform.position.x, lastRb.transform.position.y, lastRb.transform.position.z - (cl.length + xOffset));
            cl.connectedRigidbody = lastRb;
            //if (i > 0) {
            //    links[i - 1].downNeighbor = cl;
            //    cl.upNeighbor = links[i - 1];
            //}
            links.Add(cl);
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
