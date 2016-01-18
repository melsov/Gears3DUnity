using UnityEngine;
using System.Collections;

public class TubeEntrance : MonoBehaviour {

    protected Tube tube;
    public float strength = 10f;
    
    void Awake() {
        tube = GetComponentInParent<Tube>();
        UnityEngine.Assertions.Assert.IsTrue(tube != null);
    }

    void OnTriggerEnter(Collider other) {
        other.GetComponent<Rigidbody>().useGravity = false;
        //pullToCenter(other);
    }

    //void OnTriggerStay(Collider other) {
    //    //pullToCenter(other);
    //}

    //void OnTriggerExit(Collider other) {
    //    //pullToCenter(other);
    //}

    private void pullToCenter(Collider other) {
        Vector3 towards = transform.position - other.transform.position;
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;
        rb.velocity = Vector3.Lerp(towards.normalized, rb.velocity.normalized, .2f) * rb.velocity.magnitude;
        //if (towards.z < Mathf.Epsilon * -1f && rb.velocity.z < Mathf.Epsilon * -1f) {
        //}
    }
}
