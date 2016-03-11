using UnityEngine;
using System.Collections;

public class ChainLink : MonoBehaviour {

    HingeJoint hj;

	// Use this for initialization
	void Awake() {
        hj = GetComponent<HingeJoint>();
	
	}

    public float length {
        get { return 1.05f; }
    }

    public Rigidbody connectedRigidbody {
        get { return hj.connectedBody; }
        set {
            hj.connectedBody = value;
            hj.autoConfigureConnectedAnchor = true;
            Vector3 dif = hj.connectedBody.transform.position - transform.position;
            hj.anchor = new Vector3(dif.x, 0f, dif.z);
        }
    }

    public Rigidbody rigidbod {
        get { return GetComponent<Rigidbody>(); }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
