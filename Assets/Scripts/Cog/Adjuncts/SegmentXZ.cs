﻿using UnityEngine;
using System.Collections.Generic;

public struct SegmentXZ  {

    public VectorXZ start;
    public VectorXZ end;

    public SegmentXZ(VectorXZ _start, VectorXZ _end) {
        start = _start; end = _end;
    }

    public VectorXZ distance {
        get { return end - start; }
    }
	
    public IEnumerable<VectorXZ> intervalCoordinates(float intervalSize) {
        float magSquared = distance.magnitudeSquared;
        VectorXZ norm = distance.normalized;
        for(int i = 0;  ; ++i) {
            float intervalDistance = i * intervalSize;
            if (intervalDistance * intervalDistance > magSquared) { break; }
            yield return start + norm * intervalDistance;
        }
    }
}
