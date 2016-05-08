using UnityEngine;
using System.Collections;

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

    public static T FindInCog<T>(Transform trans) {
        Cog cog = trans.GetComponentInParent<Cog>();
        if (cog == null) { return default(T); }
        return cog.GetComponentInChildren<T>();
    }
}
