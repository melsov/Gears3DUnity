using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CursorInteraction : MonoBehaviour {

    private VectorXZ mouseLocal;
    private CursorInteractable[] interactables;
    private bool _shouldOverrideDrag;

	void Awake () {
        List<CursorInteractable> cis = new List<CursorInteractable>();
        MonoBehaviour[] mbs = GetComponents<MonoBehaviour>();
        foreach(MonoBehaviour mb in mbs) {
            if (mb is CursorInteractable) {
                cis.Add((CursorInteractable) mb);
            }
        }
        interactables = cis.ToArray();
	}	

    public void mouseDown(VectorXZ worldPoint) {
        _shouldOverrideDrag = false;
        foreach (CursorInteractable ci  in interactables) {
            ci.startCursorInteraction(worldPoint);
            _shouldOverrideDrag = ci.shouldOverrideDrag(worldPoint);
        }
        mouseLocal = worldPoint - new VectorXZ(transform.position); 
    }

    public virtual void drag(VectorXZ worldPoint) {
        foreach (CursorInteractable ci in interactables) {
            ci.cursorInteracting(worldPoint);
        }
        if (!_shouldOverrideDrag) {
            transform.position = (worldPoint - mouseLocal).vector3(transform.position.y);
        }
    }

    public void mouseUp(VectorXZ worldPoint) {
        foreach (CursorInteractable ci in interactables) {
            ci.endCursorInteraction(worldPoint);
        }
    }
}

public interface CursorInteractable
{
    void startCursorInteraction(VectorXZ cursorGlobal);
    bool shouldOverrideDrag(VectorXZ cursorGlobal);
    void cursorInteracting(VectorXZ cursorGlobal);
    void endCursorInteraction(VectorXZ cursorGlobal);
}
