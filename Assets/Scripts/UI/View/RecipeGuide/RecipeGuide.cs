using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class RecipeGuide : MonoBehaviour {

    [SerializeField]
    private RectTransform mainPanel;
    [SerializeField]
    private RecipeEntryPanel recipeEntryPrefab;
    [SerializeField]
    private ShowHide showHide;

    public void Awake() {
        showHide.onShow = showRecipes;
        //showRecipes();
    }
    
	public void showRecipes() {
        throw new System.NotImplementedException("need to convert to Ordered recipe lookup");
        if (GetComponentsInChildren<RecipeEntryPanel>().Length > 1) { return; }
        float lastHeight = -50f; 
        print(recipeEntryPrefab.GetComponent<RectTransform>().rect.position.y);
        float heightIncrement = recipeEntryPrefab.GetComponent<RectTransform>().rect.height;
        foreach(PrefabRecipe pr in RecipeLookup.Instance.getPrefabRecipes()) {
            createEntry(pr, lastHeight);
            lastHeight -= heightIncrement;
        }
        recipeEntryPrefab.gameObject.SetActive(false);
    }

    private void createEntry(PrefabRecipe pr, float lastHeight) {
        RecipeEntryPanel rep = Instantiate<RecipeEntryPanel>(recipeEntryPrefab);
        rep.gameObject.SetActive(true);
        rep.transform.SetParent(recipeEntryPrefab.transform.parent);
        rep.amount1.text = string.Format("{0}", pr.ingredients[0].amount);
        rep.ingredient1.sprite = pr.ingredients[0].prefab.sprite;
        rep.amount2.text = string.Format("{0}", pr.ingredients[1].amount);
        rep.ingredient2.sprite = pr.ingredients[1].prefab.sprite;
        rep.result.sprite = pr.result.GetComponent<Combinable>().sprite;
        RectTransform rt = rep.GetComponent<RectTransform>();
        Vector2 anch = rt.anchoredPosition;
        anch.y = lastHeight;
        anch.x = 0f;
        rt.anchoredPosition = anch;
    }
}
