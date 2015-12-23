using UnityEngine;
using System.Collections;

public class TransformUtil : MonoBehaviour
{
    public static void ParentToAndAlignXZ(Transform child, Transform parent, Transform localOffsetObject) {
        AlignXZ(child, parent, localOffsetObject);
        child.transform.SetParent(parent, true);
    }

    public static void AlignXZ(Transform child, Transform parent, Transform localOffsetObject) {
        Vector3 localOffset = localOffsetObject == null ? Vector3.zero : child.transform.rotation * localOffsetObject.transform.localPosition;
        child.transform.position = new Vector3(
            -localOffset.x +
            parent.position.x,
            child.transform.position.y,
            -localOffset.z +
            parent.position.z);
    }
}
