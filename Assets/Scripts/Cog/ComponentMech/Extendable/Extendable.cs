using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Extendable : MonoBehaviour {

    protected LineSegment lineSegment;
    private PoleSectionProvider poleSectionProvider;
    [SerializeField]
    protected ExtendableSection poleSectionPrefab;
    [SerializeField]
    protected AnimationCurve placementDistribution;
    [SerializeField]
    protected Transform extendToTarget;

    public void Awake() {
        lineSegment = GetComponentInChildren<LineSegment>();
        poleSectionProvider = new PoleSectionProvider(this);
        poleSectionPrefab.transform.gameObject.SetActive(false);
    }

    protected Vector3 direction {
        get { return lineSegment.distance.normalized.vector3(); }
    }

    public void extendTo(Vector3 global) {
        VectorXZ online = lineSegment.closestPointOnLine(global);
        VectorXZ dif = online - lineSegment.startXZ;
        if (dif.magnitude < .05f) {
            lineSegment.setDistance(.05f);
        } else {
            lineSegment.end.position = online.vector3(transform.position.y);
        }
        poleSectionProvider.extend(lineSegment.distance.magnitude);
    }

    public void FixedUpdate() {
        extendTo(extendToTarget.position);
    }

    protected class PoleSectionProvider
    {
        private Transform container { get { return extendable.transform; } }
        private ExtendableSection original { get { return extendable.poleSectionPrefab; } }
        private List<ExtendableSection> sections = new List<ExtendableSection>();
        private int activeSections;
        private const float MAX_SCALED_SECTIONS = 8f;
        private readonly float targetAverageSectionLength;
        private AnimationCurve placementLookup { get { return extendable.placementDistribution; } }
        private WeakReference _extendable = new WeakReference(null);
        private Extendable extendable { get { return (Extendable)_extendable.Target; } }
        public const int SECTIONS = 10;

        public PoleSectionProvider(Extendable extendable) { 
            _extendable = new WeakReference(extendable);
            targetAverageSectionLength = original.length * .6f;
            setup();
            UnityEngine.Assertions.Assert.IsFalse(targetAverageSectionLength == 0f, "no length??");
        }

        public float maxDistance { get { return targetAverageSectionLength * SECTIONS;  } }

        public void setup() {
            float minScaleDif = .35f;
            for(int i = 0; i < SECTIONS; ++i) {
                ExtendableSection sec = createSection();
                float scaler = 1f - minScaleDif + Mathf.Min(minScaleDif * i / MAX_SCALED_SECTIONS, minScaleDif);
                sec.transform.localScale = new Vector3(1f, scaler, scaler);
                sections.Add(sec);
            }
        }

        //private void enableSections(int count) {
        //    if (activeSections == count) { return; }
        //    int limit = Mathf.Max(count, sections.Count);
        //    for(int i = 0; i < limit; ++i) {
        //        ExtendableSection section;
        //        if (i >= sections.Count) {
        //            section = createSection();
        //            sections.Add(section);
        //        } else {
        //            section = sections[i];
        //        }
        //        section.transform.gameObject.SetActive(i < count);
        //        float scaler = 1f - Mathf.Min(.3f * i / MAX_SCALED_SECTIONS, .3f);
        //        section.transform.localScale = new Vector3(1f, scaler, scaler);
        //    }
        //    activeSections = count;
        //}
        
/* TODO: make extension smooth */
        public void extend(float totalDistance) {
            for(int i = 0; i < SECTIONS; ++i) {
                float placementScale = placementLookup.Evaluate(i / (float)SECTIONS);
                sections[i].transform.position = container.transform.position + extendable.direction * placementScale * totalDistance;
            }
        }

        private ExtendableSection createSection() {
            Transform t = Instantiate<Transform>(original);
            ExtendableSection result = new ExtendableSection();
            result.transform = t;
            result.transform.parent = container;
            return result;
        }

        
    }
}

[System.Serializable]
public class ExtendableSection
{
    public Transform transform;
    public float length = 1f;

    public static implicit operator Transform(ExtendableSection es) { return es.transform; }
}
