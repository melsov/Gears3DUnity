using UnityEngine;
using System.Collections.Generic;
using System;

public class Combiner : MonoBehaviour {

    //List<Combinable> combinables = new List<Combinable>();
    public Transform outTube;

    protected CombinerSlot[] slots;

	void Awake () {
        slots = GetComponentsInChildren<CombinerSlot>();
	}

    protected void eject() {
        
    }

    protected void combine(Transform combined) {
        foreach(CombinerSlot slot in slots) {
            slot.release();
        }
        Transform result = Instantiate<Transform>(combined);
        // TODO: result position = in the output tube
        result.position = outTube.position;
        Debug.LogError("combined: " + result.name);
    }

    public void evaluate() {
        Recipe recipe = new Recipe();
        foreach(CombinerSlot slot in slots) {
            recipe.add(slot.typeAmount);
        }
        Transform result = null;
        RecipeState state = RecipeLookup.Instance.lookup(recipe, ref result);
        switch (state) {
            case RecipeState.POTENTIALLY_vALID:
            default:
                break;
            case RecipeState.VALID:
                combine(result);
                eject();
                break;
            case RecipeState.INVALID:
                eject();
                break;
        }
    }
}
