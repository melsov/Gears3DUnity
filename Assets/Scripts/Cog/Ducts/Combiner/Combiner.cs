using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
//TODO: save / restore combiner
/*
 * TODO: the tubes connected to combiner should become composite cogs with combiner? <--no
 * INSTEAD: there's a cog called 'HasBackSocket', its owns various ducts, etc. and duct is no longer a Cog at all
 *  */
 
public class Combiner : Cog {

    public Transform outTube;
    public Transform ejectArea;
    public Collider _mainCollider;
    protected Combinable defaultRock;
    protected CombinerMultiSlot multiSlot;

    private bool isBaking;

    protected override void awake() {
        base.awake();
        defaultRock = Resources.LoadAll<Rock>("Prefabs/Dispensables")[0];
        multiSlot = GetComponentInChildren<CombinerMultiSlot>();
	}

    protected void combine(Transform combined) {
        Transform result = Instantiate<Transform>(combined);
        result.position = outTube.position;
        AudioManager.Instance.play(this, AudioLibrary.CombinerSoundName);
    }

    protected void destroyIngredients() {
        multiSlot.clear();
    }

    public void evaluate() {
        if (isBaking) { return; }
        OrderedRecipe recipe = multiSlot.getOrderedRecipe();
        Transform result = null;

        RecipeState state = OrderedRecipeLookup.Instance.lookup(recipe, ref result);
        switch (state) {
            case RecipeState.POTENTIALLY_VALID:
            default:
                break;
            case RecipeState.VALID:
                StartCoroutine(bake(.5f, result));
                break;
            case RecipeState.INVALID:
                bakeDefaultRock();
                break;
        }
    }

    protected IEnumerator bake(float bakingTime, Transform result) {
        if (!isBaking) {
            isBaking = true;
            yield return new WaitForSeconds(bakingTime);
            combine(result);
            destroyIngredients();
            isBaking = false;
        } else {
            yield return null;
        }
    }
    protected void bakeDefaultRock() {
        StartCoroutine(bake(.3f, defaultRock.transform));
    }

    #region cog implementation

    public override ConnectionSiteAgreement.ConnektAction connektActionAsTravellerFor(ContractSpecification specification) {
        return ConnectionSiteAgreement.doNothing;
    }

    public override ProducerActions producerActionsFor(Cog client, ContractSpecification specification) {
        return ProducerActions.getDoNothingActions();
    }

    public override ClientActions clientActionsFor(Cog producer, ContractSpecification specification) {
        return ClientActions.getDoNothingActions();
    }

    protected override ContractSiteBoss getContractSiteBoss() {
        return ContractSiteBoss.emptyBoss();
    }

    #endregion
}
