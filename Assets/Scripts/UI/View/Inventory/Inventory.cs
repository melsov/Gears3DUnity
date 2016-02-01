using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Collections;
using System;

public class Inventory : Singleton<Inventory> {

    protected Category[] categories;
    protected CanvasGroup mainPanel;
    public RectTransform categoryParentPanel;
    public RectTransform categorySelectPanel;
    public Button categorySelectButtonPrefab;
    protected CursorInput cursorInput;

    protected Inventory() { }

	void Awake () {
        categories = GetComponentsInChildren<Category>();
        mainPanel = GetComponent<CanvasGroup>();
        cursorInput = Camera.main.GetComponent<CursorInput>();
        setupCategorySelect();
        if (categories.Length > 0) {
            showCategory(categories[0]);
        }
	}

    private void setupCategorySelect() {
        Vector3 anchor = new Vector3(4,-4,4);
        foreach(Category cat in categories) {
            createCategorySelectButton(cat, anchor);
            anchor.y -= categorySelectButtonPrefab.GetComponent<RectTransform>().rect.height + 2;
        }
    }

    private void createCategorySelectButton(Category cat, Vector3 anchoredPosition) {
        Button cb = Instantiate<Button>(categorySelectButtonPrefab);
        cb.GetComponentInChildren<Text>().text = cat.title;
        cb.GetComponent<Image>().color = cat.GetComponent<Image>().color;
        cb.onClick.AddListener(delegate { showCategory(cat); });
        cb.transform.SetParent(categorySelectPanel);
        cb.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
    }

    public void showCategory(Category category) {
        hideAllCateories();
        RectTransform panel = category.getPanel();
        panel.gameObject.SetActive(true);
        panel.transform.SetParent(categoryParentPanel.transform);
    }

    private void hideAllCateories() {
        foreach(Category cat in categories) {
            cat.gameObject.SetActive(false);
        }
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

