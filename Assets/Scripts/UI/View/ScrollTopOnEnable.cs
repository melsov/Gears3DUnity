using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScrollTopOnEnable : MonoBehaviour {

	public void OnEnable() {
        print("scroll up");
        GetComponent<ScrollRect>().verticalScrollbar.value = 1f;
    }
}
