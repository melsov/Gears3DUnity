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
}
