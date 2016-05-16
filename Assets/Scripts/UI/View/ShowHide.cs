using UnityEngine;

public class ShowHide : MonoBehaviour {

    public bool hideOnAwake = true;
    public delegate void OnShow();
    public OnShow onShow;
    [SerializeField]
    private GameObject target;

    public void Awake() {
        if (target == null) {
            target = gameObject;
        }
        if (hideOnAwake) {
            hide();
        }
    }

	public void hide() {
        target.SetActive(false);
    }

    public void show() {
        target.SetActive(true);
        if (onShow != null)
            onShow();
        else
            print("on show null");
    }
}
