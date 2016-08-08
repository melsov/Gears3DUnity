using UnityEngine;
using System.Collections;

//TODO: impl this.
//Also: make a wheel (or is that just a hinge joint??) like hinge joint but with more front pegs
public class MagneticPlaceable : Placeable {

//TODO: ignore other magnets while placed as a client: use isKinematic
    protected override void onReceiveProducer(Cog producer, ContractSpecification specification) {
    }

    protected override void onBeAbsolvedOfProducer(Cog producer, ContractSpecification specification) {
    }

}
