using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Gear : MonoBehaviour , CursorInteractable, Spinnable {

    public float xSpeed = 10f;
    public float angularVelocity = 20f;
    public Vector3 orientation = Vector3.up;

    private AxelSocket[] axelSockets;
    private AxelSocket connectedAxelSocket;

    private bool isCursorInteracting;

    public bool isDrivenFromAxel {
        get {
            return connectedAxelSocket != null && connectedAxelSocket.axel != null;
        }
    }  

	void Awake () {
        axelSockets = GetComponentsInChildren<AxelSocket>();
        //TODO: generate sockets programmatically
	}

    public void setSocketClosestsToAxel(Axel axel) {
        Vector3 dist = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        foreach(AxelSocket socket in axelSockets) {
            Vector3 nextdDist = socket.transform.position - axel.transform.position;
            if (nextdDist.magnitude < dist.magnitude) {
                dist = nextdDist;
                connectedAxelSocket = socket;
            }
        }
        connectedAxelSocket.axel = axel;
        positionOnParent();
    }

    private void positionOnParent() {
        if (connectedAxelSocket == null) {
            return;
        }
        transform.rotation = Quaternion.identity;
        connectedAxelSocket.axel.transform.rotation = Quaternion.identity;
        transform.position = new Vector3(
            -connectedAxelSocket.transform.localPosition.x +
            connectedAxelSocket.axel.transform.position.x, 
            transform.position.y,
            -connectedAxelSocket.transform.localPosition.z +
            connectedAxelSocket.axel.transform.position.z);
        transform.SetParent(connectedAxelSocket.axel.transform, true);
    }

    private void detachFromAxel() {
        transform.SetParent(null);
        connectedAxelSocket = null;
    }
	
	void Update () {
        if (isDrivenFromAxel) {
            return;
        }
        Vector3 pos = transform.position;
        pos.x += xSpeed * Time.deltaTime * Input.GetAxis("Horizontal");
        transform.position = pos;
	}

    private void turnWithAxel() {
        transform.RotateAround(connectedAxelSocket.transform.position, orientation, connectedAxelSocket.axel.axisRotation);
    }

    public void turnByDegrees(float d) { 
        transform.Rotate(Vector3.up * d * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other) {
        if (isDrivenFromAxel) return;
        print("on t enter");
        setSocket(getAxel(other));
    }

    void OnTriggerStay(Collider other) {
        if (isDrivenFromAxel) return;
        setSocket(getAxel(other));
    }

    private Axel getAxel(Collider other) {
        Axel anAxel = other.gameObject.GetComponent<Axel>();
        if (anAxel == null) {
            anAxel = other.gameObject.GetComponentInParent<Axel>();
        }
        return anAxel;
    }

    private void setSocket(Axel anAxel) {
        if (anAxel != null) {
            if (isCursorInteracting) {
                //TODO: tell axel to highlight itself?
            } else {
                setSocketClosestsToAxel(anAxel);
            }
        }
    }

    public void startCursorInteraction() {
        detachFromAxel();
        isCursorInteracting = true;
    }

    public void cursorInteracting() {
        
    }

    public void endCursorInteraction() {
        print("end cursor interaction");
        isCursorInteracting = false;
    }

    public Spin receiveSpin(Spin incomingSpin) {
        throw new NotImplementedException();
    }
}

public struct Spin
{
    public float radius;
    public float angularVelocity;

    public Spin(float _radius, float _angularVelocity) {
        radius = _radius; angularVelocity = _angularVelocity;
    }
}

public interface Spinnable
{
    Spin receiveSpin(Spin incomingSpin);
}
