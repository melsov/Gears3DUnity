using UnityEngine;
using System.Collections.Generic;

public class LinearHandleSet : HandleSet {

    LineSegment lineSegment;
    protected Dictionary<Transform, Handle> lookup = new Dictionary<Transform, Handle>();

    void Awake() {
        if (transform.parent != null) {
            lineSegment = transform.parent.GetComponentInChildren<LineSegment>();
        } else {
            lineSegment = GetComponentInChildren<LineSegment>();
        }
        lineSegment.adjustedExtents += adjustedExtents;
        UnityEngine.Assertions.Assert.IsTrue(handles.Length == 2, "Linear Handle Set needs exactly two handles");
        Handle h = handles[0];
        Vector3 startDif = lineSegment.start.position - h.transform.position;
        Vector3 endDif = lineSegment.end.position - h.transform.position;
        if (startDif.sqrMagnitude < endDif.sqrMagnitude) {
            lookup.Add(lineSegment.start, h);
            lookup.Add(lineSegment.end, handles[1]);
        } else {
            lookup.Add(lineSegment.start, handles[1]);
            lookup.Add(lineSegment.end, h);
        }
    }

    private void adjustedExtents() {
        lookup[lineSegment.start].transform.position = lineSegment.start.position;
        lookup[lineSegment.end].transform.position = lineSegment.end.position;
    }
}
