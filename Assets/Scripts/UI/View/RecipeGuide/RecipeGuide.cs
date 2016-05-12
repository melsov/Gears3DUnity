using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class RecipeGuide : MonoBehaviour {

    [SerializeField]
    private RectTransform mainPanel;
    [SerializeField]
    private RecipeEntryPanel recipeEntryPrefab;

    public void Start() {
        showRecipes();
    }
    
	public void showRecipes() {
        float lastHeight = -50f; // recipeEntryPrefab.GetComponent<RectTransform>().rect.position.y;
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
        //Vector3 pos = recipeEntryPrefab.GetComponent<RectTransform>().rect.position;
        //pos.y = lastHeight;
        //pos.x = 0f;
        //rep.GetComponent<RectTransform>().position = pos;
    }
}
