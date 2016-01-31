using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Collections;

public class Inventory : Singleton<Inventory> {

    public Category[] categories;
    protected CanvasGroup mainPanel;
    public RectTransform categoryParentPanel;
    protected CursorInput cursorInput;

    protected Inventory() { }

	void Awake () {
        mainPanel = GetComponent<CanvasGroup>();
        cursorInput = Camera.main.GetComponent<CursorInput>();
        if (categories.Length > 0) {
            showCategory(categories[0]);
        }
	}

    public void showCategory(Category category) {
        RectTransform panel = category.getPanel();
        panel.transform.SetParent(categoryParentPanel.transform);
    }

    public static void createButton(RectTransform instantiatePanelPrefab, Sprite sprite, string title, Transform t, Transform prefab, Vector3 anchoredPosition) {
        RectTransform instPanel = Instantiate<RectTransform>(instantiatePanelPrefab);
        InstantiateButton b = instPanel.GetComponentInChildren<InstantiateButton>();
        b.prefab = prefab;
        b.instantiateItem = Inventory.Instance.instantiatePrefab;
        b.GetComponent<Image>().sprite = sprite;
        instPanel.GetComponentInChildren<Text>().text = title;
        RectTransform rt = instPanel.GetComponent<RectTransform>();
        instPanel.transform.SetParent(t);
        rt.localScale = new Vector3(1, 1, 1);
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = anchoredPosition;
    }	

    private void instantiatePrefab(Transform prefab) {
        print("inst pressed");
        Transform tr = Instantiate<Transform>(prefab);
        tr.position = cursorInput.mousePositionOnRootPegboard;
        CursorInteraction ci = tr.GetComponent<CursorInteraction>();
        Assert.IsTrue(ci != null);
        cursorInput.takeInteractable(ci);
    }
}

[System.Serializable]
public struct InventoryItem
{
    public Sprite sprite;
    public Transform prefab;
    public string title;
}

