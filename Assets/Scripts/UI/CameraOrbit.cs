using UnityEngine;
using System.Collections;
using System;

public class CameraOrbit : MonoBehaviour , IBinaryStateButtonClient {

    protected Vector3 swivelled = new Vector3(45f, 35f, 0f);
    protected Vector3 normal = new Vector3(90f, 0f, 0f);
    private bool isSwivelled;

    private Vector3 offset;
    public void Awake() {
        offset = Camera.main.transform.position;
    }

    private Vector3 targetPosition {
        get {
            return isSwivelled ? swivelled : normal;
        }
    }
    
	
    public void toggleSwivel() {
        isSwivelled = !isSwivelled;
        StartCoroutine(swivel());
    }
    //private Vector3 cameraEuler {
    //    get { return Camera.main.GetComponent<RectTransform>().eulerAngles; }
    //    set { Camera.main.GetComponent<RectTransform>().eulerAngles = value; }
    //}
    //private Vector3 cameraBaseBoardPoint {
    //    get { return new VectorXZ(Camera.main.transform.position).vector3(0); }
    //}
    //private Vector3 distanceFromBaseBoard {
    //    get { return cameraBaseBoardPoint - Camera.main.transform.position; }
    //}
    //private Vector3 forward {
    //    get {
    //        return Camera.main.transform.forward;
    //    }
    //}
    //private float cameraToBaseBoardMagnitude {
    //    get {
    //        return Mathf.Abs(Camera.main.transform.position.y / (forward.y));
    //    }
    //}
    private Vector3 lookPosition {
        get { return Camera.main.transform.position + Camera.main.transform.rotation * offset; }
    }
    
    //private Vector3 deltaSwivel {
    //    get { return normal - swivelled * (isSwivelled ? -1f : 1f); }
    //}
    private Vector3 axis {
        get { return new Vector3(-Mathf.Sin(swivelled.x * Mathf.Deg2Rad), 0f, Mathf.Sin(swivelled.y * Mathf.Deg2Rad)); }
    }
    private float swivelAngleDegrees {
        get { return isSwivelled ? 45f : -45f; } //NOTE: imprecise
    }
    
    private IEnumerator swivel() {
        int incr = 36;
        Quaternion camRo = Camera.main.transform.rotation;
        Quaternion target = Quaternion.Euler(targetPosition);
        Vector3 look = lookPosition;
        for (int i = 0; i < incr; ++i) {
            Camera.main.transform.RotateAround(look, axis, swivelAngleDegrees * (1/(float)incr)); 
            //Camera.main.transform.rotation = Quaternion.Slerp(Camera.main.transform.rotation, target, .2f); //WANT?
            yield return new WaitForFixedUpdate();
        }
        //Camera.main.transform.rotation = target; //WANT?
    }
	void Update () {
        
	}

    public bool getState() {
        return isSwivelled;
    }

    public BinaryStateButton.PressAction getPressAction() {
        return toggleSwivel;
    }
}
