using UnityEngine;
using System.Collections;

public class RayCastUtil : MonoBehaviour {

    public static Collider getColliderUnderCursor(out RaycastHit rayHit) {
        return getColliderUnderCursor(~0, out rayHit);
    }

    public static Collider getColliderUnderCursor(int layerMask, out RaycastHit rayHit) {
        Collider result = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out rayHit, 100f, layerMask)) {
            result = rayHit.collider;
        }
        return result;
    }
}
