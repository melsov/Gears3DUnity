using UnityEngine;
using System.Collections;
using System;

public class TestCog : Cog , ICursorAgentClient
{
    public void OnTriggerEnter(Collider other) {
        print(" Test cog T Enter: " + other.name);
    }

    public void OnTriggerStay(Collider other) {
        print(" T Stay " + other.name);
    }

    #region cog-abstract-impl
    public override ClientActions clientActionsFor(Cog producer, ContractSpecification specification) {
        return ClientActions.getDoNothingActions();
    }

    public override ConnectionSiteAgreement.ConnektAction connektActionAsTravellerFor(ContractSpecification specification) {
        return ConnectionSiteAgreement.doNothing;
    }

    public void dragOverride(VectorXZ cursorGlobal) {
    }

    public void endDragOverride(VectorXZ cursorGlobal) {
    }

    public void handleTriggerEnter(Collider other) {
        print("got trigger enter with " + other.name + " : " + FindCog(other.transform).name);
    }

    public Collider mainCollider() {
        return GetComponentInChildren<Collider>();
    }

    public bool makeConnectionWithAfterCursorOverride(Collider other) {
        return false;
    }

    public void onDragEnd() {
    }

    public override ProducerActions producerActionsFor(Cog client, ContractSpecification specification) {
        return ProducerActions.getDoNothingActions();
    }

    public Collider shouldPreserveConnection() {
        return null;
    }

    public void startDragOverride(VectorXZ cursorGlobal, Collider dragOverrideCollider) {
    }

    public void suspendConnection() {
    }

    public void triggerExitDuringDrag(Collider other) {
    }

    protected override ContractSiteBoss getContractSiteBoss() {
        return new ContractSiteBoss(null);
    }

    #endregion

}
