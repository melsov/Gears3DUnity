using UnityEngine;
using System.Collections;

public enum CardinalDirection
{
    NORTH, EAST, SOUTH, WEST
}

public enum OrientationType
{
    SELF_MATCHING, OPPOSITE_CARDINAL_DIRECTION
}

public struct SiteOrientation {
    private OrientationType orientationType;
    private int cardinalDirection;
    private SiteOrientation(OrientationType ot, int cardinalDirection_) {
        orientationType = ot;
        cardinalDirection = cardinalDirection_;
    }
    
    public bool canAlignWith(SiteOrientation other) {
        if (orientationType == OrientationType.SELF_MATCHING) {
            return other.orientationType == OrientationType.SELF_MATCHING;
        }
        return (cardinalDirection + 2) % 4 == other.cardinalDirection;
    }

    public static SiteOrientation selfMatchingOrientation() {
        return new SiteOrientation(OrientationType.SELF_MATCHING, -9999999);
    }

    public static SiteOrientation OrientedOrientation(CardinalDirection cd) {
        return new SiteOrientation(OrientationType.OPPOSITE_CARDINAL_DIRECTION, (int)cd);
    }
}
