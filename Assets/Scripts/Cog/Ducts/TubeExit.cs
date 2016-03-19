using UnityEngine;
using System.Collections;

public class TubeExit : MonoBehaviour {

    protected Tube tube;
    public float strength = 10f;
    
    void Awake() {
        tube = GetComponentInParent<Tube>();
        UnityEngine.Assertions.Assert.IsTrue(tube != null);
    }

    void OnTriggerExit(Collider other) {
        if (other.GetComponent<Rigidbody>() == null) { return; }
        other.GetComponent<Rigidbody>().useGravity = true;
    }

}
