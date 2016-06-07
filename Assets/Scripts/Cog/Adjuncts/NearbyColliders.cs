using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class NearbyColliders {
    public static IEnumerable<Collider> nearbyColliders(Collider subject, int resolution) {
        foreach(Collider c in nearbyColliders(subject, resolution, ~0)) {
            yield return c;
        }
    }

    public static IEnumerable<Collider> nearbyColliders(Collider subject, int resolution, LayerMask mask) {
        float radius = subject.bounds.extents.x + .1f;
        int subjectLayer = subject.gameObject.layer;
        RaycastHit rch;
        for (int index = 0; index < resolution; ++index) {
            Vector3 origin = subject.transform.position + PerimeterXZ.directions(resolution)[index].vector3() * radius;
            origin.y = Camera.main.transform.position.y;
            Vector3 dir = Camera.main.transform.forward;
            Ray ray = new Ray(origin, dir);
            Debug.Log(LayerLookup.IgnoreRaycast);
            subject.gameObject.layer = LayerLookup.IgnoreRaycast;
            if (Physics.Raycast(ray, out rch, 100f, mask)) {
                subject.gameObject.layer = subjectLayer;
                yield return rch.collider;
            }
        }
    }

}
