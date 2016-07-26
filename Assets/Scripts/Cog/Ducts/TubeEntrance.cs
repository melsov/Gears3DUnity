using UnityEngine;
using System.Collections;

public class TubeEntrance : MonoBehaviour {

    //protected Tube tube;
    //public float strength = 10f;
    
    //void Awake() {
    //    tube = GetComponentInParent<Tube>();
    //    UnityEngine.Assertions.Assert.IsTrue(tube != null);
    //}

    void OnTriggerEnter(Collider other) {
        if (other.GetComponent<Rigidbody>() == null) { return; }
        other.GetComponent<Rigidbody>().useGravity = false;
    }
   
    //private void pullToCenter(Collider other) {
    //    Vector3 towards = transform.position - other.transform.position;
    //    Rigidbody rb = other.GetComponent<Rigidbody>();
    //    if (rb == null) { return; }
    //    rb.velocity = Vector3.Lerp(towards.normalized, rb.velocity.normalized, .2f) * rb.velocity.magnitude;
    //}
}
