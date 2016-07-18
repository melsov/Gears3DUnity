using UnityEngine;
using System.Collections;
using System;

public enum CardinalDirection
{
    NORTH, EAST, SOUTH, WEST
}

public enum OrientationType
{
    ALIGNS_WITH_NOTHING, SELF_MATCHING, OPPOSITE_CARDINAL_DIRECTION, FORWARD_FACING, BACKWARD_FACING
}

public class SiteOrientation {

    private OrientationType orientationType;
    private int cardinalDirection;
    //public Transform transform;
    //private WeakReference _cog;
    //public Cog cog {
    //    get { return (Cog)_cog.Target; }
    //}
    //public CogContract contract; //TODO: JK: connectionSiteAgreements can/should go back to own ContractSites (not SiteOrientations)

    private SiteOrientation(OrientationType ot, int cardinalDirection_) {
        orientationType = ot;
        cardinalDirection = cardinalDirection_;
        //_cog = new WeakReference(cog);
    }

    public override string ToString() {
        return string.Format("Site Orientation: {0}, {1}", orientationType, (orientationType == OrientationType.OPPOSITE_CARDINAL_DIRECTION ? cardinalDirection : -1));
    }

    public bool canAlignWith(SiteOrientation other) {
        switch (orientationType) {
            case OrientationType.ALIGNS_WITH_NOTHING:
                return false;
            case OrientationType.SELF_MATCHING:
                return other.orientationType == OrientationType.SELF_MATCHING;
            case OrientationType.FORWARD_FACING:
                return other.orientationType == OrientationType.BACKWARD_FACING;
            case OrientationType.BACKWARD_FACING:
                return other.orientationType == OrientationType.FORWARD_FACING;
            case OrientationType.OPPOSITE_CARDINAL_DIRECTION:
            default:
                return (cardinalDirection + 2) % 4 == other.cardinalDirection;
        }
    }

    public static SiteOrientation selfMatchingOrientation() {
        return new SiteOrientation(OrientationType.SELF_MATCHING, -9999999);
    }

    public static SiteOrientation OrientedOrientation(CardinalDirection cd) {
        return new SiteOrientation(OrientationType.OPPOSITE_CARDINAL_DIRECTION, (int)cd);
    }

    public static SiteOrientation fromRigidRelationshipConstraint(RigidRelationshipConstraint rrc) {
        return new SiteOrientation(rrc == RigidRelationshipConstraint.CAN_ONLY_BE_CHILD ? OrientationType.BACKWARD_FACING : OrientationType.FORWARD_FACING, -9999999);
    }

    public static SiteOrientation alignsWithNothing() {
        return new SiteOrientation(OrientationType.ALIGNS_WITH_NOTHING, -99999999);
    }
}
