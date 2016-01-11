using UnityEngine;
using System.Collections;

//CONSIDER: Crafting system
public abstract class Dispensable : MonoBehaviour {

	// Use this for initialization
	void Awake () {
        awake();
	}
    protected virtual void awake() {

    }

    protected virtual bool shouldDestroy() {
        return false; 
    }
}
