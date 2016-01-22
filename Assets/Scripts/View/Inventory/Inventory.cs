using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Inventory : MonoBehaviour {

    public InventorySection root;
    protected CanvasGroup mainPanel;
    public Button testButtonPrefab;

	void Awake () {
        mainPanel = GetComponent<CanvasGroup>();
        Button b = GameObject.Instantiate<Button>(testButtonPrefab);
        RectTransform rt = b.GetComponent<RectTransform>();
        b.transform.SetParent(transform);
        rt.localScale = new Vector3(1, 1, 1);
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = Vector3.zero;
	}
	
	
}

[System.Serializable]
public struct InventorySection
{
    public Transform[] prefabs; // TODO: change to array of "Inventory Items" (new struct with prefab, text, image)
    public InventorySection[] subsections;
    public string title;
}
