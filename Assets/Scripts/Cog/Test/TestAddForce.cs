using UnityEngine;
using System.Collections;
using System;

public class TestAddForce : MonoBehaviour {
    Rigidbody rb;
    private bool pausedAlready;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        //StartCoroutine(forces());
        StartCoroutine(testPrediction());
        //StartCoroutine(report());
	}

    private IEnumerator forces() {
        Vector3 start = rb.position;
        Vector3 targetPosition = Vector3.right * 10000f - start;
        float tInterval = 5f;
        for(int i = 0; i < 3000; ++i) {
            yield return new WaitForFixedUpdate();
            Vector3 frameTargetPos = rb.position + targetPosition * Time.fixedDeltaTime / tInterval;
            forceForTargetPosition(frameTargetPos);
            if (Time.fixedTime > tInterval && !pausedAlready) {
                pausedAlready = true;
                UnityEditor.EditorApplication.isPaused = true;
            }
        }
    }

    void FixedUpdate () {
        //forceForSpeed(10f);

	}

    private IEnumerator testPrediction() {
        float maxDelta = 0f;
        for (int i = 0; i < 1000; ++i) {
            Vector3 force = Vector3.right * 20f;
            Vector3 pred = TransformUtil.distanceOneFrameGiven(rb, force);
            Vector3 prePos = transform.position;
            rb.AddForce(force);
            yield return new WaitForFixedUpdate();
            Vector3 actualDist = transform.position - prePos;
            maxDelta = Mathf.Max(maxDelta, Mathf.Abs(actualDist.magnitude - pred.magnitude));
            if (i % 22 == 0) print(maxDelta);
        }
        
    }

    private IEnumerator report() {
        for(int i = 0; i < 500; ++i) {
            print(string.Format(" vel {0} , time {1} ", GetComponent<Rigidbody>().velocity.x, Time.time));
            yield return new WaitForSeconds(Time.maximumDeltaTime * 5f);
        }

    }

    private void forceForSpeed(float desiredSpeed) {
        Vector3 forwardSpeed = (rb.transform.right * desiredSpeed);
        Vector3 force = (forwardSpeed.normalized * rb.mass * desiredSpeed);
        force = (rb.drag * force) / (1 - 0.02f * rb.drag);
        rb.AddForce(force);
    }

    private float drag { get { return 1f - Time.fixedDeltaTime * rb.drag; } }

    private void forceNoDrag(float targetVel) {
        Vector3 force = (rb.transform.right * (targetVel - Vector3.Dot(rb.transform.right, rb.velocity))) * rb.mass;
        rb.AddForce(force);
    }

    private Vector3 forceForVelocity(Vector3 targetVel) {
        Vector3 force = (targetVel / drag - rb.velocity) * rb.mass / Time.fixedDeltaTime;
        return force;
    }

    private void forceForTargetPosition(Vector3 targetPosition) {
        Vector3 dist = targetPosition - rb.position;
        Vector3 targetVel = dist / Time.fixedDeltaTime;
        Vector3 f = forceForVelocity(targetVel);
        rb.AddForce(f);
    }
}
