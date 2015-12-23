using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class CursorInput : MonoBehaviour {

    public GameObject testThing;
    private RaycastHit rayHit;
    public LineRenderer line;

    private CursorInteraction ci;

    private int layerMask;
    private int dragOverrideMask;

    // Use this for initialization
    void Awake () {
        line = GetComponent<LineRenderer>();
        layerMask = ~(LayerMask.GetMask("DragOverride") | LayerMask.GetMask("CogComponent"));
        dragOverrideMask = LayerMask.GetMask("DragOverride");
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Fire1")) {
            getInteractable();
        }
        if (Input.GetButton("Fire1")) {
            if (ci != null) {
                Vector3 v = Input.mousePosition;
                v = Camera.main.ScreenToWorldPoint(new Vector3(v.x, v.y, Camera.main.nearClipPlane + 4));
                ci.drag(new VectorXZ(v));
            }
        }
        if (Input.GetButtonUp("Fire1")) {
            if (ci != null) {
                Vector3 v = Input.mousePosition;
                v = Camera.main.ScreenToWorldPoint(new Vector3(v.x, v.y, Camera.main.nearClipPlane + 4));
                ci.mouseUp(new VectorXZ(v));
            }
            releaseInteractable();
        }
	}

    public void releaseInteractable() {
        ci = null;
    }

    private void getInteractable() {
        releaseInteractable();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        line.SetPosition(0, ray.origin);
        if (Physics.Raycast(ray, out rayHit, 100f, dragOverrideMask)) {
            ci = rayHit.collider.GetComponent<CursorInteraction>();
            if (ci == null) {
                ci = TransformUtil.FindTypeInParentRecursive<CursorInteraction>(rayHit.collider.transform, 4);
            }
        }

        if (ci == null) {
            if (Physics.Raycast(ray, out rayHit, 100f, layerMask)) {
                ci = rayHit.collider.GetComponent<CursorInteraction>();
                if (ci == null) {
                    ci = TransformUtil.FindTypeInParentRecursive<CursorInteraction>(rayHit.collider.transform, 4);
                }
            }
        }
        if (ci == null) {
            return;
        }
        ci.mouseDown(new VectorXZ(rayHit.point));
        line.SetPosition(1, ray.origin + ray.direction * 20f);
    }

    // NOT IN USE
    private void createObjectAtCursor(GameObject ob) {
        Vector3 mouse = Input.mousePosition;
        Vector3 v = Camera.main.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, Camera.main.nearClipPlane + 4));
        Instantiate(ob, v, Quaternion.identity);
    }
}
