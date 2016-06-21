using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class NearbyColliders {

    public static IEnumerable<Collider> nearbyColliders(Collider subject, float resolution) {
        foreach(Collider c in nearbyColliders(subject, resolution, 1f)) {
            yield return c;
        }
    }

    public static IEnumerable<Collider> nearbyColliders(Collider subject, float resolution, float expandExtentsBy) {
        foreach(Collider c in nearbyColliders(subject, resolution, ~0)) {
            yield return c;
        }
    }

    public static IEnumerable<Collider> nearbyColliders(Collider subject, float resolution, LayerMask mask, float expandExtentsBy) {
        BoundsXZ b = BoundsXZ.fromCollider(subject);
        b.extendExtents(expandExtentsBy);
        foreach (Collider c in b.overlappingColliders(resolution, mask)) {
            yield return c;
        }
    }

}
