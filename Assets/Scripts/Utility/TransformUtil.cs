using UnityEngine;
using System.Collections;

public class TransformUtil : MonoBehaviour
{
    public static void ParentToAndAlignXZ(Transform child, Transform parent, Transform localOffsetObject) {
        AlignXZ(child, parent, localOffsetObject);
        child.transform.SetParent(parent, true);
    }

// CONSIDER: this seems to fail sometimes? maybe do some tests... use local scale as well?
    public static void AlignXZ(Transform child, Transform parent, Transform localOffsetObject) {
        Vector3 localOffset = Vector3.zero;
        if (localOffsetObject != null) {
            localOffset = localOffsetObject.transform.localPosition;
            localOffset.Scale(child.transform.localScale);
            localOffset = child.transform.rotation * localOffset;
        }

        child.transform.position = new Vector3(
            -localOffset.x +
            parent.position.x,
            child.transform.position.y,
            -localOffset.z +
            parent.position.z);
    }
    //haha: Get Component in Parent is recursive already
    //public static T FindTypeInParentRecursive<T>(Transform t, int levelLimit) {
    //    if (levelLimit <= 0 || t == null ) { return default(T); }
    //    T result = t.GetComponentInParent<T>();
    //    if (result == null) {
    //        return FindTypeInParentRecursive<T>(t.parent, --levelLimit);
    //    }
    //    return result;
    //}
}
