using UnityEngine;
using System.Collections;

public class Motor : MonoBehaviour {

    public float angularVelocity = 10f;
    public bool isPowered = true;
    public Axel axel;

    private float angle;
    
	void Awake () {
        axel = GetComponentInChildren<Axel>();
	}

	void Update () {
        if (!isPowered) {
            return;
        }
        angle += angularVelocity * Time.deltaTime;
        axel.turnTo(angle);
	}
}
