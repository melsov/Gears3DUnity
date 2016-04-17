using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;

//TODO: random concept: drill into ground to get resources: 'oil derrick'

public class Inventory : Singleton<Inventory> {

    protected Category[] categories;
    protected CanvasGroup mainPanel;
    public RectTransform categoryParentPanel;
    public RectTransform categorySelectPanel;
    public Button categorySelectButtonPrefab;
    protected CursorInput cursorInput;

    private List<Transform> sceneObjects = new List<Transform>();

    public ScrollRect itemScrollRect;
    //TODO: force horiz scroll to turn off

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

    public void putBackInInventory(Transform trans) {
        Destroy(trans.gameObject);
    }

    private void setupCategorySelect() {
        Vector3 anchor = new Vector3(4,-4,1);
        foreach(Category cat in categories) {
            createCategorySelectButton(cat, anchor);
            anchor.y -= categorySelectButtonPrefab.GetComponent<RectTransform>().rect.height + 2;
        }
    }

    private void createCategorySelectButton(Category cat, Vector3 anchoredPosition) {
        Button cb = cat.selectButton; // Instantiate<Button>(categorySelectButtonPrefab);
        cb.GetComponentInChildren<Text>().text = cat.title;
        cb.GetComponent<Image>().color = cat.GetComponent<Image>().color;
        cb.onClick.AddListener(delegate { showCategory(cat); });
        ////cb.transform.SetParent(categorySelectPanel);
        //cb.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
    }

    public void showCategory(Category category) {
        hideAllCateories();
        RectTransform panel = category.getPanel();
        panel.gameObject.SetActive(true);
        panel.transform.SetParent(categoryParentPanel.transform);
        itemScrollRect.content.position = new Vector3(itemScrollRect.content.position.x, 0f, itemScrollRect.content.position.z);
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
        Transform tr = Instantiate<Transform>(prefab);
        setPosition(tr);
        CursorInteraction ci = tr.GetComponent<CursorInteraction>();
        Assert.IsTrue(ci != null, "no cursor interaction on prefab: " + prefab.name);
        cursorInput.takeInteractable(ci);
    }

    private void setPosition(Transform t) {
        t.position = cursorInput.mousePositionOnRootPegboard;
        TransformUtil.PositionOnYLayer(t);
    }

    public Transform prefabWithId(int id_) {
        foreach(Category cat in categories) {
            foreach(InventoryItem ii in cat.inventoryItems) {
                print(ii.title);
                ItemID itemID = ii.prefab.GetComponent<ItemID>();
                if (itemID != null && id_ == itemID.id) {
                    print("found prefab with id: " + ii.prefab.name);
                    return ii.prefab;
                }
            }
        }
        print("prefab not found");
        return null;
    }
}

[System.Serializable]
public struct InventoryItem
{
    public Sprite sprite;
    public Transform prefab;
    public string title;
}

