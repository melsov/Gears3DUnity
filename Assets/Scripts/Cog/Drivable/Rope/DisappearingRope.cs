using UnityEngine;
using System.Collections;

public class DisappearingRope : Rope {

    [SerializeField]
    protected Collider disappearEntrance;

    protected override void awake() {
        base.awake();
        
    }
    protected override HingeChainLink getLinkInstance() {
        return Instantiate<DisappearingHingeChainLink>((DisappearingHingeChainLink) linkPrefab);
    }

    protected DisappearingHingeChainLink disappearPrefab {
        get { return (DisappearingHingeChainLink)linkPrefab; }
    }

    protected DisappearingHingeChainLink this[int index] {
        get {
            if (index < 0 || index >= links.Count) { return null; }
            return (DisappearingHingeChainLink)links[index];
        }
    }
}
