using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class Combiner : MonoBehaviour {

    //List<Combinable> combinables = new List<Combinable>();
    public Transform outTube;

    protected CombinerSlot[] slots;

	void Awake () {
        slots = GetComponentsInChildren<CombinerSlot>();
	}

    protected void eject() {
        foreach(Combinable c in combinables()) {
            //TODO: c.transform.position = eject area position
            c.enable();
        }
        //resetSlots();
    }

    public IEnumerable<Combinable> combinables() {
        foreach(CombinerSlot cs in slots) {
            foreach (Combinable c in cs.combinables()) {
                yield return c;
            }
        }
    }

    protected void combine(Transform combined) {
        print("combine");
        foreach (Combinable com in combinables()) {
            print(com.name);
        }
        Transform result = Instantiate<Transform>(combined);
        // TODO: result position = in the output tube
        result.position = outTube.position;
        Debug.LogError("combined: " + result.name);
    }

    protected void destroyIngredients() {
        foreach(Combinable c in combinables()) {
            Destroy(c.gameObject);
        }
        resetSlots();
    }
    protected void resetSlots() {
        foreach (CombinerSlot slot in slots) {
            slot.release();
        }
    }

    public void evaluate() {
        Recipe recipe = new Recipe();
        foreach(CombinerSlot slot in slots) {
            if (slot.empty) { return; }
            recipe.add(slot.typeAmount);
        }
        Transform result = null;
        RecipeState state = RecipeLookup.Instance.lookup(recipe, ref result);
        switch (state) {
            case RecipeState.POTENTIALLY_vALID:
            default:
                print("po valid");
                break;
            case RecipeState.VALID:
                combine(result);
                destroyIngredients();
                break;
            case RecipeState.INVALID:
                print("invalid");
                eject();
                break;
        }
    }
}
