using UnityEngine;
using System.Collections;

//CONSIDER: Crafting system
public abstract class Dispensable : Combinable {

	void Awake () {
        awake();
	}
    protected virtual void awake() {

    }

    void Update() {
        update();
    }
    protected virtual void update() {
        if (shouldDestroy()) {
            Destroy(gameObject);
        }
    }

    protected virtual bool shouldDestroy() {
        return transform.position.magnitude > 3000f;
    }
}
