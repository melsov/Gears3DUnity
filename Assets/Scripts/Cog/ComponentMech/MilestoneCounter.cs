using UnityEngine;
using System.Collections;

public class MilestoneCounter
{
    private float total;
    private delegate float GetValue(float input);
    private GetValue _getValue;
    public bool useAbsoluteValue {
        set {
            if (value) {
                _getValue = delegate (float input) { return Mathf.Abs(input); };
            } else {
                _getValue = delegate (float input) { return input; };
            }
        }
    }

    public delegate void OnHitMilestone();
    private OnHitMilestone onHitMilestone;

    public void register(OnHitMilestone ohm) {
        onHitMilestone += ohm;
    }
    public void unregister(OnHitMilestone ohm) {
        onHitMilestone -= ohm;
    }

    public MilestoneCounter(float interval, bool useAbsoluteValue) {
        this.interval = interval;
        this.useAbsoluteValue = useAbsoluteValue;
    }

    private float interval;

    public void updateTotal(float next) {
        int prev = Mathf.FloorToInt( _getValue(total / interval));
        int nextFloor = Mathf.FloorToInt(_getValue(next / interval));
        if (nextFloor > prev) {
            onHitMilestone();
        }
        total = next;
    }
    
}
