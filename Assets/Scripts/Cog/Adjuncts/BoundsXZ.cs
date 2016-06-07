using UnityEngine;
using System.Collections.Generic;

public struct BoundsXZ  {

    public VectorXZ min {
        get { return new VectorXZ(bounds.min); }
    }
    public VectorXZ max {
        get { return new VectorXZ(bounds.max); }
    }

    public VectorXZ minXmaxZ {
        get { return new VectorXZ(bounds.min.x, bounds.max.z); }
    }
    public VectorXZ maxXminZ {
        get { return new VectorXZ(bounds.max.x, bounds.min.z); }
    }

    public VectorXZ extents {
        get { return new VectorXZ(bounds.extents); }
    }

    public Bounds bounds;

    public BoundsXZ(Bounds _bounds) {
        bounds = _bounds;
    }

    public IEnumerable<VectorXZ> gridCoordinates(float gridSize) {
        SegmentXZ over = new SegmentXZ(min, maxXminZ);
        SegmentXZ up = new SegmentXZ(min, minXmaxZ);
    }
	
    
}
