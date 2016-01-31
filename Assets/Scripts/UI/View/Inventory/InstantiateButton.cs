using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InstantiateButton : Button { // TODO: button implements cursor interaction

    public Transform prefab;
    public delegate void InstantiateItem(Transform item);
    public InstantiateItem instantiateItem;

    public ScrollRect containerScrollRect {
        get {
            return GetComponentInParent<ScrollRect>();
        }
    }

    public void handleMouseDown() {
        setScrollRectEnabled(false);
    }

    public void handleDragEnteredScene() {
        if (instantiateItem != null) {
            instantiateItem(prefab);
        }
    }

    public void handleMouseUp() {
        setScrollRectEnabled(true);
    }

    private void setScrollRectEnabled(bool enabled) {
        if (containerScrollRect != null) {
            containerScrollRect.horizontal = enabled;
            containerScrollRect.vertical = enabled;
        }
    }

    public Sprite proxySprite() {
        return GetComponent<Image>().sprite;
    }

    //protected override void Awake() {
    //    base.Awake();
    //    onClick.AddListener(delegate { handleClick();  });
    //}
}
