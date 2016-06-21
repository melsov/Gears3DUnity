using UnityEngine;
using System.Collections;

public class TestBoundsXZ : MonoBehaviour {

    BoundsXZ b;

	void Start () {
        transform.rotation = Quaternion.Euler(new Vector3(0f, 45f, 0f));
        b = BoundsXZ.fromCollider(GetComponent<Collider>()); // new BoundsXZ(GetComponent<Renderer>());
        b.extendExtents(1.3f);
        int i = 0;
        foreach(Collider c in b.overlappingColliders(.5f)) {
            BugLine.Instance.markPoint(c.transform.position, i++);
        }
	}

    public void FixedUpdate() {
        //BugLine.Instance.clear();
        //int i = 0;
        //foreach (VectorXZ v in b.gridCoordinates(1f)) {
        //    BugLine.Instance.markPoint(v.vector3(), i++);
        //}

    }


}
