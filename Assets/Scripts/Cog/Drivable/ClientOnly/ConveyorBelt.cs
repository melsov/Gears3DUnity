using UnityEngine;
using System.Collections.Generic;
using System;

public class ConveyorBelt : GearDrivenMechanism , ICollisionProxyClient // Drivable , GearDrivable
{
    protected LineSegment lineSegment;
    protected float _radius;
    protected List<Collision> collisions = new List<Collision>();
    protected AngleStep wheelRotation;
    public float speedMultiplier = 10f;
    private float damper = 1000f;

    protected RotationIndicator[] rotationIndicators;
    public Transform beltTransform;

    protected override Transform dragOverrideTarget {
        get {
            return beltTransform;
        }
    }

    protected override float radius {
        get {
            if (_radius == 0f) {
                _radius = GetComponentInChildren<CollisionProxy>().transform.localScale.z / 2f;
            }
            return _radius;
        }
    }

    protected float beltSpeed {
        get {
            return wheelRotation.deltaAngle * radius * speedMultiplier / damper;
        }
    }

    protected override void awake() {
        base.awake();
        lineSegment = GetComponentInChildren<LineSegment>();
        rotationIndicators = GetComponentsInChildren<RotationIndicator>();
        if (rotationIndicators == null) { rotationIndicators = new RotationIndicator[0]; }
    }

    protected override void updateMechanism(Drive drive) {
        wheelRotation.update(wheelRotation.getAngle() + drive.amount * -1f / radius);
        foreach (RotationIndicator ri in rotationIndicators) {
            ri.rotation = wheelRotation.getAngle();
        }
    }

    public void FixedUpdate() {
        for (int i = 0; i < collisions.Count; ++i) {
            Collision coll = collisions[i];
            if (coll == null || coll.rigidbody == null) {
                collisions.Remove(coll);
                --i;
                continue;
            }
            coll.rigidbody.MovePosition(coll.transform.position + lineSegment.normalized.vector3() * beltSpeed);
        }
    }



    public override float driveScalar() {
        return 0f;
    }

    #region ICollisionProxyClient
    public void proxyCollisionEnter(Collision collision) {
        collision.rigidbody.velocity = Vector3.zero;
        collision.rigidbody.useGravity = false;
        collisions.Add(collision);
    }

    public void proxyCollisionStay(Collision collision) {
    }

    public void proxyCollisionExit(Collision collision) {
        if (collision != null) {
            collision.rigidbody.useGravity = true;
        }
        collisions.Remove(collision);
    }
    #endregion

    #region contract
    protected override ContractNegotiator getContractNegotiator() {
        return new ConveyorBeltContractNegotiator(this);
    }

    public class ConveyorBeltContractNegotiator : ContractNegotiator
    {
        public ConveyorBeltContractNegotiator(Cog cog_) : base(cog_) {
        }

        protected override List<ContractSpecification> orderedContractPreferencesAsOfferer(Cog cogForTypeWorkaround) {
            List<ContractSpecification> result = new List<ContractSpecification>();
            if (cogForTypeWorkaround is Gear) {
                result.Add(new ContractSpecification(CogContractType.DRIVER_DRIVEN, RoleType.CLIENT));
            }
            return result;
        }
    }

    protected override ViableContractLookup getViableContractLookup() {
        return new ViableConveyorBeltContractLookup(this);
    }

    public class ViableConveyorBeltContractLookup : ViableDrivableContractLookup
    {
        public ViableConveyorBeltContractLookup(Cog cog_) : base(cog_) {
        }

        protected override void setupLookups() {
            asClientLookup.Add(CogContractType.DRIVER_DRIVEN, delegate (Cog other) {
                return other.GetComponent<Gear>();
            });

            //asProducerLookup.Add(CogContractType.DRIVER_DRIVEN, delegate (Cog other) {
            //    return other.GetComponent<Gear>();
            //}); // would be cool if conveyor belts could drive other conveyor belts (TODO)
        }
    }

    protected override UniqueClientConnectionSiteBoss getUniqueClientSiteConnectionSiteBoss() {
        return new UniqueClientConnectionSiteBoss(
            /* 1.) client site */
            new KeyValuePair<ClientOnlyCTARSet, ExclusionarySiteSet>(
                ClientOnlyCTARSet.drivenSet(),
                new ExclusionarySiteSet(new ContractSite(this, SiteOrientation.selfMatchingOrientation()))
                )
            );
    }
    #endregion
}
