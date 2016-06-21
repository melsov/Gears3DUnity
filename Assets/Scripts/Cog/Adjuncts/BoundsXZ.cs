using UnityEngine;
using System.Collections.Generic;

public struct BoundsXZ  {

    public VectorXZ rotatedMin {
        get {
            return trans.position - rotatedExtents;
        }
    }

    public VectorXZ rotatedExtents {
        get { return trans.rotation * unrotatedExtents; }
    }

    //private Transform trans {
    //    get { return rendrr.transform; }
    //}

    public VectorXZ rotatedMax {
        get {
            return trans.position + rotatedExtents; 
        }
    }

    public VectorXZ minXmaxZ {
        get {
            return trans.position + trans.rotation * new Vector3(-unrotatedExtents.x, 0f, unrotatedExtents.z);
        }
    }

    public VectorXZ maxXminZ {
        get {
            return trans.position + trans.rotation * new Vector3(unrotatedExtents.x, 0f, -unrotatedExtents.z);
        }
    }

    public Transform trans;
    public Vector3 unrotatedExtents;
    public static BoundsXZ fromRenderer(Renderer rendrr) {
        return fromTransform(rendrr.transform, false);
    }

    public static BoundsXZ fromCollider(Collider collider) {
        return fromTransform(collider.transform, true);
    }

    private static BoundsXZ fromTransform(Transform t, bool colliderBounds) {
        Quaternion ro = t.rotation;
        t.rotation = Quaternion.identity;
        Vector3 extents = colliderBounds ? t.GetComponent<Collider>().bounds.extents : t.GetComponent<Renderer>().bounds.extents;
        extents.y = 0f;
        BoundsXZ result = new BoundsXZ(t, extents);
        t.rotation = ro;
        return result;
    }

    private BoundsXZ(Transform trans, Vector3 extents) {
        this.trans = trans;
        this.unrotatedExtents = extents;
    }

    public void extendExtents(float f) {
        unrotatedExtents *= f;
    }
    public void addToExtents(Vector3 v) {
        unrotatedExtents += v;
    }

    public VectorXZ[] corners {
        get {
            return new VectorXZ[] {
                rotatedMin, minXmaxZ, rotatedMax, maxXminZ
            };
        }
    }

    public IEnumerable<VectorXZ> gridCoordinates(float gridUnitSize) {
        SegmentXZ up = new SegmentXZ(rotatedMin, minXmaxZ);
        SegmentXZ over = new SegmentXZ(rotatedMin, maxXminZ);
        foreach(VectorXZ vertical in up.intervalCoordinates(gridUnitSize, true)) {
            foreach(VectorXZ horizontal in over.intervalCoordinates(gridUnitSize, true)) {
                yield return rotatedMin + vertical + horizontal;
            }
        }
    }
    public HashSet<Collider> overlappingColliders(float gridUnitSize) {
        return overlappingColliders(gridUnitSize, LayerLookup.AllLayers);
    }

    public HashSet<Collider> overlappingColliders(float gridUnitSize, LayerMask mask) {
        HashSet<Collider> result = new HashSet<Collider>();
        Ray ray;
        RaycastHit rh;
        foreach(VectorXZ v in gridCoordinates(gridUnitSize)) {
            ray = new Ray(v.vector3(Camera.main.transform.position.y), -EnvironmentSettings.towardsCameraDirection);
            if (Physics.Raycast(ray, out rh, 300f, mask)) {
                result.Add(rh.collider);
            }
        }
        return result;
    }
	
    
}
