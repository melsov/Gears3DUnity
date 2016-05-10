using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
//TODO: save / restore combiner

public class Combiner : Drivable {

    public Transform outTube;
    public Transform ejectArea;
    public Collider _mainCollider;

    protected CombinerSlot[] slots;

    protected override void awake() {
        base.awake();
        slots = GetComponentsInChildren<CombinerSlot>();
	}

    protected override Collider vMainCollider() {
        return _mainCollider;
    }

    protected void eject() {
        foreach(Combinable c in combinables()) {
            c.transform.position = ejectArea.position;
            c.enable();
        }
        resetSlots();
    }

    public IEnumerable<Combinable> combinables() {
        foreach(CombinerSlot cs in slots) {
            foreach (Combinable c in cs.combinables()) {
                yield return c;
            }
        }
    }

    protected void combine(Transform combined) {
        Transform result = Instantiate<Transform>(combined);
        result.position = outTube.position;
        AudioManager.Instance.play(this, AudioLibrary.CombinerSoundName);
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
        if (isBaking) { return; }
        Recipe recipe = new Recipe();
        foreach(CombinerSlot slot in slots) {
            recipe.add(slot.typeAmount);
        }
        Transform result = null;
        RecipeState state = RecipeLookup.Instance.lookup(recipe, ref result);
        switch (state) {
            case RecipeState.POTENTIALLY_VALID:
            default:
                break;
            case RecipeState.VALID:
                StartCoroutine(bake(recipe, result));
                break;
            case RecipeState.INVALID:
                eject();
                break;
        }
    }
    protected bool isBaking;
    protected IEnumerator bake(Recipe recipe, Transform result) {
        if (!isBaking) {
            isBaking = true;
            yield return new WaitForSeconds(recipe.bakeTimeSeconds);
            combine(result);
            destroyIngredients();
            isBaking = false;
        } else {
            yield return null;
        }
    }

    public override float driveScalar() {
        return 0f;
    }

    public override Drive receiveDrive(Drive drive) {
        return drive;
    }
}
