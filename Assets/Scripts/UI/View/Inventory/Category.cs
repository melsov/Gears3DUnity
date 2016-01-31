using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Category : MonoBehaviour {

    protected RectTransform rectTransform;
    protected Inventory inventory;
    public InventoryItem[] inventoryItems;
    public RectTransform templateButtonPanel;

	void Awake () {
        rectTransform = GetComponent<RectTransform>();
        inventory = GetComponentInParent<Inventory>();
	}	
    
    public RectTransform getPanel() {
        Button[] buttons = rectTransform.GetComponentsInChildren<Button>();
        if (buttons.Length > 0) {
            return rectTransform;
        }
        Vector3 pos = new Vector3(0, -10);
        foreach (InventoryItem ii in inventoryItems) {
            Inventory.createButton(templateButtonPanel, ii.sprite, ii.title, transform, ii.prefab, pos);
            pos.y -= getTemplateButtonHeight();
        }

        return rectTransform;
    }

    public void hide() {
        gameObject.SetActive(false);
    }

    public void show() {
        gameObject.SetActive(true);
    }

    private float getTemplateButtonHeight() {
        return templateButtonPanel.rect.height;
    }
}
