using UnityEngine;
using System;
using System.Collections.Generic;

public class RecipeLookup : Singleton<RecipeLookup> {

    [SerializeField]
    protected PrefabRecipe[] prefabRecipes;
    protected Recipe[] recipes;
    
    public IEnumerable<PrefabRecipe> getPrefabRecipes() {
        foreach(PrefabRecipe pr in prefabRecipes) {
            yield return pr;
        }
    }

    void Awake() {
        recipes = new Recipe[prefabRecipes.Length];
        int index = 0;
        foreach(PrefabRecipe pr in prefabRecipes) {
            Recipe r = new Recipe();
            r.resultPrefab = pr.result;
            foreach(PrefabAmount pa in pr.ingredients) {
                TypeAmount ta = new TypeAmount(pa.prefab.GetComponent<Combinable>().GetType(), pa.amount);
                r.add(ta);
            }
            recipes[index++] = r;
        }
    }

    public RecipeState lookup(Recipe recipe, ref Transform result) {
        bool allInvalid = true;
        foreach (Recipe r in recipes) {
            RecipeState rs = r.compare(recipe);
            if (rs == RecipeState.VALID) {
                result = r.resultPrefab;
                return RecipeState.VALID;
            }
            if (rs == RecipeState.POTENTIALLY_VALID) {
                allInvalid = false;
            }
        }
        if (allInvalid) { return RecipeState.INVALID; }
        return RecipeState.POTENTIALLY_VALID;
    }

    public GameObject instantiateType(Type type) {
        return null;
    }
}

[Serializable]
public class Recipe
{
    protected List<TypeAmount> ingredients = new List<TypeAmount>();
    public float bakeTimeSeconds = 1f;
    public Combinable[] byproducts;

    public void add(TypeAmount typeAmount) {
        ingredients.Add(typeAmount);
    }
    public Transform resultPrefab;

    public RecipeState compare(Recipe other) {
        bool allValid = true;
        foreach(TypeAmount ta in other.ingredients) {
            RecipeState rs = findIngredient(ta);
            if (rs == RecipeState.INVALID) {
                return RecipeState.INVALID;
            }
            if (rs == RecipeState.POTENTIALLY_VALID) {
                allValid = false;
            }
        }
        if (allValid) { return RecipeState.VALID; }
        
        return RecipeState.POTENTIALLY_VALID;
    }
    private RecipeState findIngredient(TypeAmount typeAmount) {
        foreach(TypeAmount ta in ingredients) {
            if (ta.compare(typeAmount) == RecipeState.VALID) {
                return RecipeState.VALID;
            }
            if (ta.compare(typeAmount) == RecipeState.POTENTIALLY_VALID) {
                return RecipeState.POTENTIALLY_VALID;
            }
        }
        return RecipeState.INVALID;
    }
}

public enum RecipeState
{
    POTENTIALLY_VALID, VALID, INVALID
};

[Serializable]
public struct PrefabRecipe
{
    public PrefabAmount[] ingredients;
    public Transform result;
}

[Serializable]
public struct PrefabAmount
{
    public Combinable prefab;
    public int amount;
}

public struct TypeAmount
{
    public Type type;
    public int amount;

    public TypeAmount(Type _type, int _amount) {
        type = _type;
        amount = _amount;
    }

    public override bool Equals(object obj) {
        return base.Equals(obj);
    }

    public override int GetHashCode() {
        return base.GetHashCode();
    }

    public RecipeState compare(TypeAmount other) {
        if (other.type == null) { return RecipeState.POTENTIALLY_VALID; }
        if (!other.type.Equals(type) || other.amount > amount) {
            return RecipeState.INVALID;
        }
        if (other.amount == amount) {
            return RecipeState.VALID;
        }
        return RecipeState.POTENTIALLY_VALID;

    }
}