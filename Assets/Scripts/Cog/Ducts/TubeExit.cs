using UnityEngine;
using System.Collections;

public class TubeExit : MonoBehaviour {

    protected Tube tube;
    public float strength = 10f;
    
    void Awake() {
        tube = GetComponentInParent<Tube>();
        UnityEngine.Assertions.Assert.IsTrue(tube != null);
    }

    //void OnTriggerEnter(Collider other) {
    //    other.GetComponent<Rigidbody>().useGravity = false;
    //    //pullToCenter(other);
    //}

    //void OnTriggerStay(Collider other) {
    //    //pullToCenter(other);
    //}

    void OnTriggerExit(Collider other) {
        other.GetComponent<Rigidbody>().useGravity = true;
    }

}
