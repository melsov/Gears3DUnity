using UnityEngine;
using System.Collections;

public class HingeJointPlaceable : Placeable {

    protected HingeJoint _hingeJoint;

    protected override void awake() {
        base.awake();
        _hingeJoint = GetComponentInChildren<HingeJoint>();
    }

    public override ProducerActions producerActionsFor(Cog client, ContractSpecification specification) {
        ProducerActions actions = base.producerActionsFor(client, specification);
        
        if (specification.contractType == CogContractType.PARENT_CHILD) {
            actions.initiate = delegate (Cog cog) {

            };
            
        }
        return actions;
    }

    private IEnumerator calmDownHingeJoint() {
        yield return null;
    }
}
