using UnityEngine;
using System.Collections;
using System;

/*
Base class for all mechanisms
*/
[System.Serializable]
public class Cog : MonoBehaviour {

    void OnDestroy() {
        vOnDestroy();
    }

    protected virtual void vOnDestroy() {
        //AudioManager.Instance.remove(this);
    }

    public static Cog FindCog(Transform trans) {
        return FindInCog<Cog>(trans);
    }
    public static T FindInCog<T>(Transform trans) {
        Cog cog = trans.GetComponentInParent<Cog>();
        if (cog == null) { return default(T); }
        return cog.GetComponentInChildren<T>();
    }

    protected void highlight(Transform other) {
        Highlighter h = FindInCog<Highlighter>(other);
        if (h == null) { return; }
        h.highlight();
    }

    protected void unhighlight(Transform other) {
        Highlighter h = FindInCog<Highlighter>(other);
        if (h == null) { return; }
        h.unhighlight();
    }

    public void positionRelative(AddOn addOn) {
        print("pos rel to");
        Vector3 pos = transform.position;
        pos.x = addOn.transform.position.x;
        Transform target = addOn.transform;
        if (addOn is IProxyAddOn) {
            target = FindInCog<Drivable>(FindCog(addOn.transform).transform.parent).transform;
        }
        Collider other = FindInCog<Collider>(target);
        Vector3 extents = GetComponentInParent<Collider>().bounds.extents;
        pos.z = other != null ? Vector3.Lerp(other.bounds.min, other.bounds.min - extents, .85f).z : pos.z;
        transform.position = pos;
    }
}
