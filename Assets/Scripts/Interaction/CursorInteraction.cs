using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CursorInteraction : MonoBehaviour {

    protected VectorXZ mouseLocal;
    protected ICursorInteractable[] interactables;
    protected bool _shouldOverrideDrag;

    void Awake() {
        awake();
    }

	protected virtual void awake () {
        List<ICursorInteractable> cis = new List<ICursorInteractable>();
        MonoBehaviour[] mbs = GetComponents<MonoBehaviour>();
        foreach(MonoBehaviour mb in mbs) {
            if (mb is ICursorInteractable) {
                cis.Add((ICursorInteractable) mb);
            }
        }
        interactables = cis.ToArray();
	}	

    public virtual void mouseDown(VectorXZ worldPoint) {
        _shouldOverrideDrag = false;
        foreach (ICursorInteractable ci  in interactables) {
            ci.startCursorInteraction(worldPoint);
            _shouldOverrideDrag = ci.shouldOverrideDrag(worldPoint);
        }
        mouseLocal = worldPoint - new VectorXZ(transform.position); 
    }

    public virtual void drag(VectorXZ worldPoint) {
        foreach (ICursorInteractable ci in interactables) {
            ci.cursorInteracting(worldPoint);
        }
        if (!_shouldOverrideDrag) {
            transform.position = (worldPoint - mouseLocal).vector3(transform.position.y);
        }
    }

    public virtual void mouseUp(VectorXZ worldPoint) {
        foreach (ICursorInteractable ci in interactables) {
            ci.endCursorInteraction(worldPoint);
        }
    }

    public bool isOverridingDrag(VectorXZ worldPoint) {
        foreach (ICursorInteractable ci in interactables) {
            if (ci.shouldOverrideDrag(worldPoint)) { return true; }
        }
        return false;
    }
}

public interface ICursorInteractable
{
    void startCursorInteraction(VectorXZ cursorGlobal);
    bool shouldOverrideDrag(VectorXZ cursorGlobal);
    void cursorInteracting(VectorXZ cursorGlobal);
    void endCursorInteraction(VectorXZ cursorGlobal);
}
