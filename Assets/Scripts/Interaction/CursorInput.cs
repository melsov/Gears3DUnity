using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.Assertions;
using System;

public class CursorInput : MonoBehaviour {

    private RaycastHit rayHit;

    private CursorInteraction ci;
    public Image itemProxyImage; 
    public Sprite putBackInInventoryIcon;
    private bool triggeredDragEnterScene;
    private bool ciDragEnterInventory;

    private int layerMask;
    private int dragOverrideMask;
    private InstantiateButton ib;
    public bool blocked;

    // Use this for initialization
    void Awake () {
        print(System.Environment.Version);
        layerMask = ~(LayerMask.GetMask("DragOverride") | LayerMask.GetMask("CogComponent"));
        dragOverrideMask = LayerMask.GetMask("DragOverride");
        itemProxyImage.raycastTarget = false;
        itemProxyImage.enabled = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (blocked) { return; }
        if (Input.GetButtonDown("Fire1")) {
            ib = getInstantiateButton();
            if (ib != null) {
                ib.handleMouseDown();
                setProxyImage();
            }
            else if (!EventSystem.current.IsPointerOverGameObject()) {
                getInteractable();
            }
        }
        if (Input.GetButton("Fire1")) {
            if (ib != null) {
                if (!triggeredDragEnterScene && !EventSystem.current.IsPointerOverGameObject()) { // pointer is over the scene
                    ib.handleDragEnteredScene();
                    hideProxyImage();
                    triggeredDragEnterScene = true;
                } else {
                    positionProxyImage();
                }
            }
            if (ci != null) {
                if (pointerOverInventory() && !ci.isOverridingDrag(new VectorXZ(mousePositionOnRootPegboard))) {
                    if (!ciDragEnterInventory) { 
                        ciDragEnterInventory = true;
                        setRemoveFromSceneProxyImage();
                    }
                    positionProxyImage();
                } else {
                    ciDragEnterInventory = false;
                    hideProxyImage();
                }
                ci.drag(new VectorXZ(mousePositionOnRootPegboard)); 
            }
        }
        if (Input.GetButtonUp("Fire1")) {
            if (ci != null) {
                if (pointerOverInventory()) {
                    Inventory.Instance.putBackInInventory(ci.transform);
                } else {
                    ci.mouseUp(new VectorXZ(mousePositionOnRootPegboard));
                }
            }
            releaseItems();
            hideProxyImage();
        }
	}

    private void hideProxyImage() {
        itemProxyImage.enabled = false;
    }

    private void positionProxyImage() {
        if (!itemProxyImage.enabled) { return; }
        itemProxyImage.GetComponent<RectTransform>().position = Input.mousePosition;
    }

    public Vector3 mousePositionOnRootPegboard {
        get {
            Vector3 v = Input.mousePosition;
            return Camera.main.ScreenToWorldPoint(new Vector3(v.x, v.y, Camera.main.nearClipPlane + 4));
        }
    }

    public void releaseItems() {
        ci = null;
        if (ib != null) {
            ib.handleMouseUp();
        }
        ib = null;
        triggeredDragEnterScene = false;
        ciDragEnterInventory = false;
    }

    public void takeInteractable(CursorInteraction _ci) {
        ci = _ci;
        ci.mouseDown(new VectorXZ(mousePositionOnRootPegboard));
    }

    private InstantiateButton getInstantiateButton() {
        if (EventSystem.current.currentSelectedGameObject != null) {
            return EventSystem.current.currentSelectedGameObject.GetComponent<InstantiateButton>();
        }
        return null;
    }

    private bool pointerOverInventory() {
        if (EventSystem.current.IsPointerOverGameObject()) {
            return true;
        }
        return false;
    }

//TODO: make pegs take precedence over all others
    private void getInteractable() {
        releaseItems();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out rayHit, 100f, dragOverrideMask)) {
            ci = rayHit.collider.GetComponentInParent<CursorInteraction>();
        }

        if (ci == null) {
            ci = getHighestYInteractable(ray);
        }
        if (ci == null) {
            return;
        }
        ci.mouseDown(new VectorXZ(rayHit.point));
    }

    private CursorInteraction getHighestYInteractable(Ray ray) {
        HashSet<Collider> ciColliders = new HashSet<Collider>();
        int safe = 0;
        while (true) {
            if (safe++ > 5) { break; }
            if (Physics.Raycast(ray, out rayHit, 100f, layerMask)) {
                CursorInteraction ci = rayHit.collider.GetComponentInParent<CursorInteraction>();
                if (ci == null) {
                    break;
                } else {
                    ray = raySlightlyBelowHit(ray, rayHit);
                    ciColliders.Add(rayHit.collider);
                }
            }
        }

        CursorInteraction result = null;
        foreach(Collider col in ciColliders) {
            CursorInteraction ci = col.GetComponentInParent<CursorInteraction>();
            if (ci) {
                if (result == null || result.transform.position.y < ci.transform.position.y) {
                    result = ci;
                }
            }
        }
        return result;
        
    }

    public static Ray raySlightlyBelowHit(Ray ray, RaycastHit rayCastHit) {
        return new Ray(rayCastHit.point + ray.direction * .01f, ray.direction);
    }

    private void setProxyImage() {
        if (ib == null) { return; }
        itemProxyImage.enabled = true;
        itemProxyImage.gameObject.SetActive(true);
        itemProxyImage.sprite = ib.proxySprite();
    }

    private void setRemoveFromSceneProxyImage() {
        itemProxyImage.enabled = true;
        itemProxyImage.gameObject.SetActive(true);
        itemProxyImage.sprite = putBackInInventoryIcon;
    }

}
