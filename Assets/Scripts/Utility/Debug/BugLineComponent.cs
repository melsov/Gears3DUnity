using UnityEngine;
using System.Collections;

public class BugLineComponent : MonoBehaviour {

    LineRenderer lr;

    private Vector3 endPoint = Vector3.zero;
    public void setEndPoint(Vector3 ep) {
        endPoint = ep;
    }
    public void setDirection(Vector3 dir) {
        endPoint = transform.position + (dir.normalized * 3f);
    }

	void Awake () {
        lr = GetComponent<LineRenderer>();
        if (lr == null) {
            lr = gameObject.AddComponent<LineRenderer>();
        }
        lr.SetVertexCount(2);
        lr.SetColors(Color.magenta, Color.cyan);
	}
	
	// Update is called once per frame
	void Update () {
        if (!endPoint.Equals(Vector3.zero)) {
            lr.SetPositions(new Vector3[] {
                transform.position,
                endPoint
            });
        }
	}
}
