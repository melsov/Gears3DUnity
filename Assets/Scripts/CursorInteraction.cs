using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CursorInteraction : MonoBehaviour {

    private VectorXZ mouseLocal;
    private CursorInteractable[] interactables;

	void Awake () {
        MonoBehaviour[] mbs = GetComponents<MonoBehaviour>();
        List<CursorInteractable> cis = new List<CursorInteractable>();
        foreach(MonoBehaviour mb in mbs) {
            if (mb is CursorInteractable) {
                cis.Add((CursorInteractable) mb);
            }
        }
        interactables = cis.ToArray();
	}	

    public void mouseDown(VectorXZ worldPoint) {
        foreach (CursorInteractable ci  in interactables) {
            ci.startCursorInteraction();
        }
        mouseLocal = worldPoint - new VectorXZ(transform.position); 
    }

    public virtual void drag(VectorXZ worldPoint) {
        foreach (CursorInteractable ci  in interactables) {
            ci.cursorInteracting();
        }
        transform.position =(worldPoint - mouseLocal).vector3(transform.position.y);
    }

    public void mouseUp(VectorXZ xzPoint) {
        foreach (CursorInteractable ci in interactables) {
            ci.endCursorInteraction();
        }
    }
}

public interface CursorInteractable
{
    void startCursorInteraction();
    void cursorInteracting();
    void endCursorInteraction();
}
