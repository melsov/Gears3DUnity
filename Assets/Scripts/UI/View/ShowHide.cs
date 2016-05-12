using UnityEngine;

public class ShowHide : MonoBehaviour {

    public bool hideOnAwake = true;

    public void Awake() {
        if (hideOnAwake) {
            hide();
        }
    }

	public void hide() {
        this.gameObject.SetActive(false);
    }

    public void show() {
        this.gameObject.SetActive(true);
    }
}
