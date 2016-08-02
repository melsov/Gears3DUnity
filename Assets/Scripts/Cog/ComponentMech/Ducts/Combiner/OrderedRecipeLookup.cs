using UnityEngine;
using System;
using System.Collections.Generic;

public class OrderedRecipeLookup : Singleton<OrderedRecipeLookup> {

    protected OrderedRecipeLookup() { }

    public Formula[] recipes;

    public RecipeState lookup(OrderedRecipe recipe, ref Transform result) {
        bool allInvalid = true;
        foreach (Formula r in recipes) {
            RecipeState rs = OrderedRecipe.compare(recipe, r.toOrderedRecipe()); 
            if (rs == RecipeState.VALID) {
                result = r.result.transform;
                return RecipeState.VALID;
            }
            if (rs == RecipeState.POTENTIALLY_VALID) {
                allInvalid = false;
            }
        }
        if (allInvalid) { return RecipeState.INVALID; }
        return RecipeState.POTENTIALLY_VALID;
    }
    
}

public class OrderedRecipe
{
    public Combinable[] ingredients;
    internal float bakeTimeSeconds;

    public OrderedRecipe(Combinable[] ingredients) {
        this.ingredients = ingredients;
    }

    public static RecipeState compare(OrderedRecipe candidate, OrderedRecipe template) {
        if (candidate.ingredients.Length > template.ingredients.Length) { return RecipeState.INVALID; }
        for(int i = 0; i < candidate.ingredients.Length; ++i) {
            if (!Combinable.SameType(candidate.ingredients[i], template.ingredients[i])) {
                return RecipeState.INVALID;
            }
        }
        if (candidate.ingredients.Length == template.ingredients.Length) {
            return RecipeState.VALID;
        }
        return RecipeState.POTENTIALLY_VALID;
    }

}


[System.Serializable]
public struct Formula
{
    public Combinable[] orderedIngredients;
    public Combinable result;

    public OrderedRecipe toOrderedRecipe() {
        return new OrderedRecipe(orderedIngredients);
    }
}
