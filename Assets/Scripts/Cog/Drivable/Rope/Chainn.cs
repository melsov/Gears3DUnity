using UnityEngine;
using System.Collections.Generic;
using System;

public class Chainn : Drivable {

    //TO NOT DO: guide rope around gears and pegs
    //TODO: add pegboards to ends of ropes
    //TODO: create "Combiner": box with two or more inputs and an output. 
    //When dispensibles coinhabit the combiner, they fuse into a new dispensible. Dispensibles go through the combiner quickly either way.

    public ChainLink linkPrefab;
    public ChainLink baseLink;
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
        ChainLink lastCL = baseLink;
        baseLink.transform.SetParent(transform);
        for (int i = 0; i < linkCount; ++i) {
            ChainLink cl = Instantiate<ChainLink>(linkPrefab);
            cl.gameObject.SetActive(true);
            cl.transform.position = new Vector3(lastCL.transform.position.x, lastCL.transform.position.y, lastCL.transform.position.z - (cl.length + xOffset));
            cl.transform.SetParent(transform);
            if (i == 0) {
                cl.setUpNeighbor(baseLink);
                baseLink.downNeighbor = cl;
            }
            else if (i > 0) {
                cl.setUpNeighbor(links[i - 1]);
                links[i - 1].downNeighbor = cl;
            }
            links.Add(cl);
            lastCL = cl;
        }
    }

    public override float driveScalar() {
        return 0f;
    }

    public override Drive receiveDrive(Drive drive) {
        return drive;
    }

    protected override UniqueClientConnectionSiteBoss getUniqueClientSiteConnectionSiteBoss() {
        throw new NotImplementedException();
    }

    public override ConnectionSiteAgreement.ConnektAction connektActionAsTravellerFor(ContractSpecification specification) {
        throw new NotImplementedException();
    }
}

