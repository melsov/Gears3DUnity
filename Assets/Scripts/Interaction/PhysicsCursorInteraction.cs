using UnityEngine;
using System.Collections;

public class PhysicsCursorInteraction : CursorInteraction {

    protected Vector3 targetPosition;
    protected bool isInteracting;
    protected Rigidbody rb;

    protected override void awake() {
        base.awake();
        rb = GetComponent<Rigidbody>();
    }
    public override void drag(VectorXZ worldPoint) {
        foreach (ICursorInteractable ci in interactables) {
            ci.cursorInteracting(worldPoint);
        }
        if (!_shouldOverrideDrag) {
            isInteracting = true;
            targetPosition = (worldPoint - mouseLocal).vector3(transform.position.y);
        }
    }

    public override void mouseUp(VectorXZ worldPoint) {
        base.mouseUp(worldPoint);
        isInteracting = false;
    }

    void FixedUpdate() {
        if (isInteracting) {
            rb.MovePosition(targetPosition);
        }
    }

}
