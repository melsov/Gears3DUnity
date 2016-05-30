using UnityEngine;
using System.Collections.Generic;
using System;

/* HI HI
Base class for all mechanisms
*/
[System.Serializable]
public class Cog : MonoBehaviour {
    
    protected ColliderSet colliderSet;

    public void Awake() {
        awake();
    }

    protected virtual void awake() {
        if (!colliderSet) {
            colliderSet = gameObject.AddComponent<ColliderSet>();
        }
    }

    public void disableColliders() {
        colliderSet.disableForNextFixedFrame();
    }

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

    public void positionRelativeToAddOn(AddOn addOn) {
        Vector3 pos = transform.position;
        Transform target = addOn.transform;
        if (addOn is IProxyAddOn) {
            target = FindInCog<Drivable>(FindCog(addOn.transform).transform.parent).transform;
        }
        pos.x = target.position.x;
        Collider other = FindInCog<Collider>(target);
        Vector3 extents = GetComponentInParent<Collider>().bounds.extents;
        pos.z = other != null ? (other.bounds.min - extents).z : pos.z;
        transform.position = pos;
    }

    

    public static implicit operator bool(Cog exists) {
        return exists != null;
    }

}
