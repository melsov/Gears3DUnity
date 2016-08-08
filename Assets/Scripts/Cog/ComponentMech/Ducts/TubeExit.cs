using UnityEngine;
using System.Collections;

public class TubeExit : MonoBehaviour {

    

    void OnTriggerExit(Collider other) {
        if (other.GetComponent<Rigidbody>() == null) { return; }
        other.GetComponent<Rigidbody>().useGravity = true;
    }

}
