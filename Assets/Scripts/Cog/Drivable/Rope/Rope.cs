using UnityEngine;
using System.Collections.Generic;
using System;

public class Rope : MonoBehaviour {

    //TO NOT DO: guide rope around gears and pegs
    //TODO: add pegboards to ends of ropes
    //TODO: create "Combiner": box with two or more inputs and an output. 
    //When dispensibles coinhabit the combiner, they fuse into a new dispensible. Dispensibles go through the combiner quickly either way.

    public HingeChainLink linkPrefab;
    public Rigidbody baseLink;
    public int linkCount;
    public float xOffset = .05f;
    protected List<HingeChainLink> links = new List<HingeChainLink>();

    public void Awake() { awake(); }

    protected virtual void awake() {
        linkPrefab.gameObject.SetActive(false);
        construct();
    }

    private void construct() {
        if (links.Count > 0) { return; }
        Rigidbody lastRb = baseLink;
        for (int i = 0; i < linkCount; ++i) {
            HingeChainLink cl = getLinkInstance();
            cl.rope = this;
            cl.gameObject.SetActive(true);
            cl.transform.position = new Vector3(lastRb.transform.position.x, lastRb.transform.position.y, lastRb.transform.position.z - (cl.length + xOffset));
            cl.connectedRigidbody = lastRb;
            links.Add(cl);
            if (i > 0) {
                links[i - 1].downNeighbor = cl;
            }
            lastRb = cl.rigidbod;
        }
    }

    public HingeChainLink firstLink {
        get { return links[0]; }
    }
    public HingeChainLink lastLink {
        get { return links[links.Count - 1]; }
    }
    public HingeChainLink nextLastLink {
        get { if (links.Count < 2) { return null; } return links[links.Count - 2]; }
    }
    public int count { get { return links.Count; } }

    protected virtual HingeChainLink getLinkInstance() { return Instantiate<HingeChainLink>(linkPrefab); }

}
