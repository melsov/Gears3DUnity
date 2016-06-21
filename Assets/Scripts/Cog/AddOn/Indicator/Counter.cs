using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Counter : MonoBehaviour {

    public Transform mesh;
    public int increments = 10;

    protected Vector3 euler;
    private bool turning = false;
    private bool shouldTurn = true;
    public float rangeDegrees = 360f;
    protected int target;
    protected bool fulfilling;

	public void Awake() {  
        euler = mesh.eulerAngles;
	}

    protected void addRequest(int level) {
        if (level > increments - 1) { return; }
        target = level;
    }

    public void setTo(float gradient) {
        setEulerAndMeshRotation(degreesForLevel(gradient));
    }

    public void turnTo(int level) {
        addRequest(level);
        if (!fulfilling) {
            StartCoroutine(fulfill());
        }
    }

    private IEnumerator fulfill() {
        fulfilling = true;
        while(closestLevel != target) {
            StartCoroutine(notchByOne(target - closestLevel > 0));
            while(turning) {
                yield return new WaitForFixedUpdate();
            }
        }
        fulfilling = false;
    }

    protected float currentRotation { get { return euler.x; } }

    protected float gradient { get { return  Mathf.Clamp(Mathf.Abs(currentRotation) / notchDegrees, 0f, increments); } }

    protected int closestLevelForwards {
        get { return Mathf.CeilToInt(gradient); }
    }
    protected int closestLevelBackwards {
        get { return Mathf.FloorToInt(gradient); }
    }
    protected int closestLevel {
        get { return Mathf.RoundToInt(gradient); }
    }

    protected float notchDegrees { get { return rangeDegrees / (float)increments; } }

    private float degreesForLevel(int level) {
        return degreesForLevel((float)level);
    }

    private float degreesForLevel(float gradient) {
        return Mathf.Clamp(-gradient * notchDegrees, -(rangeDegrees - notchDegrees), 0f);
    }

    private IEnumerator notchByOne(bool forwards) {
        int safety = 0;
        while(turning && safety++ < 200) {
            yield return new WaitForEndOfFrame();
        }
        turning = true;
      
        float direction = forwards ? 1f : -1f;
        int currentLevel = closestLevel; 
        if (!((forwards && currentLevel > increments - 1) || (!forwards && currentLevel <= 0))) {
            float to = degreesForLevel(currentLevel + direction);
            float microStep = notchDegrees / 5f;

            int steps = Mathf.FloorToInt(Mathf.Abs(to - currentRotation) / microStep);

            for (int i = 0; i < steps; ++i) {
                setEulerAndMeshRotation(euler.x + microStep * -direction);
                yield return new WaitForFixedUpdate();
                if (!shouldTurn) { break; }
            }
            setEulerAndMeshRotation(to);
        }
       
        turning = false;
    }

    //private bool shouldNotch(bool forwards) {
        
    //}

    private void setEulerAndMeshRotation(float eulerX) {
        euler.x = eulerX;
        mesh.eulerAngles = euler;
    }
}
