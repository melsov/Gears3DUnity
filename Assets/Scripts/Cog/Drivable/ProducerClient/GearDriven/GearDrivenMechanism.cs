using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class GearDrivenMechanism : Gear {
    protected Transform gearMesh;

    protected override void awake() {
        foreach(Transform t in GetComponentsInChildren<Transform>()) {
            if (t.name.Equals("GearMesh")) {
                gearMesh = t;
                break;
            }
        }
        base.awake();
    }

    public override Drive receiveDrive(Drive drive) {
        Drive baseDrive = base.receiveDrive(drive);
        updateMechanism(baseDrive);
        return baseDrive;
    }

    protected abstract void updateMechanism(Drive drive);

    protected override DrivableConnection getDrivableConnection(Collider other) {
        if (FindInCog<Gear>(other.transform) != null) {
            return base.getDrivableConnection(other);
        }
        return new DrivableConnection(this);
    }

    protected override Transform gearTransform {
        get {
            return gearMesh;
        }
    }

    #region contract

    protected override ViableContractLookup getViableContractLookup() {
        return new ViableGearMechanismContractLookup(this);
    }

    public class ViableGearMechanismContractLookup : ViableGearContractLookup
    {
        protected GearDrivenMechanism gearDrivenMechanism {
            get { return (GearDrivenMechanism)cog; }
        }
        public ViableGearMechanismContractLookup(Cog cog_) : base(cog_) {}

        protected override void setupLookups() {
            asClientLookup.Add(CogContractType.DRIVER_DRIVEN, delegate (Cog other) {
                return other.GetComponent<Gear>();
            });
            asProducerLookup.Add(CogContractType.PARENT_CHILD, delegate (Cog other) {
                return true;
            });
        }
    }

    protected override UniqueClientContractSiteBoss getUniqueClientSiteConnectionSiteBoss() {
        UniqueClientContractSiteBoss uccsb = new UniqueClientContractSiteBoss(
            new KeyValuePair<ClientOnlyCTARSet, ExclusionarySiteSet>(
                ClientOnlyCTARSet.drivenSet(),
                new ExclusionarySiteSet(new ContractSite(this, SiteOrientation.selfMatchingOrientation())))
                );
        addConnectionSiteEntriesForFrontSocketSet(this, uccsb);
        return uccsb;
    }

    #endregion
}
