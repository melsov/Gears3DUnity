using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CursorInteraction : MonoBehaviour {

    private VectorXZ mouseLocal;
    private ICursorInteractable[] interactables;
    private bool _shouldOverrideDrag;

	void Awake () {
        List<ICursorInteractable> cis = new List<ICursorInteractable>();
        MonoBehaviour[] mbs = GetComponents<MonoBehaviour>();
        foreach(MonoBehaviour mb in mbs) {
            if (mb is ICursorInteractable) {
                cis.Add((ICursorInteractable) mb);
            }
        }
        interactables = cis.ToArray();
	}	

    public void mouseDown(VectorXZ worldPoint) {
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

    public void mouseUp(VectorXZ worldPoint) {
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
