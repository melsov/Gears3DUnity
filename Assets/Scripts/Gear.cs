using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Gear : Drivable , CursorInteractable {

    private AngleStep _angleStep;
    private AngleStep angleStep {
        get {
            if (isOnAxel()) {
                return connectedAxelSocket.axel.angleStep;
            }
            return _angleStep;
        }
    }

    public float angularVelocity {
        get {
            //if (isDrivenFromAxel()) {
                return angleStep.angularVelocity() * radius;
            //}
            //return 0f;
        }
    }

//TODO: account for tooth size 
//TODO: want radius to be a public method of Drivable: radius at position?   
    private float radius;

    public Vector3 orientation = Vector3.up;


    private AxelSocket connectedAxelSocket;

    private bool isCursorInteracting;

    private Drivable _driver;
    private List<Drivable> drivables = new List<Drivable>();

    private List<Collider> colliders = new List<Collider>();

    private float axisRotation {
        get { return transform.rotation.eulerAngles.y; }
    }

    private void updateAngleStep() {
        if (isOnAxel()) {
            _angleStep = connectedAxelSocket.axel.angleStep;
        } else {
            _angleStep.update(axisRotation);
        }

    }
    public override bool isDrivenFromAxel() {
        bool directAxel = isOnAxel();
        if (directAxel) {
            return true;
        }
        if (_driver != null && (MonoBehaviour)_driver != this) {
            return _driver.isDrivenFromAxel(); //TODO: protect against infinite recursion?
        } 
        return false; 
    }
    public bool isOnAxel() {
        return connectedAxelSocket != null && connectedAxelSocket.axel != null && transform.parent != null;
    }

    public override void awake() {
        base.awake();
        radius = GetComponent<CapsuleCollider>().radius * transform.localScale.x;
	}

    private void setSocketClosestToAxel(Axel axel) {
        connectedAxelSocket = getSocketClosestToAxel(axel);
        connectedAxelSocket.axel = axel;
        connectedAxelSocket.axel.occupied = true;
        parentToAxel();
    }

    private AxelSocket getSocketClosestToAxel(Axel axel) {
        AxelSocket closest = null;
        Vector3 dist = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        foreach(AxelSocket socket in axelSockets) {
            Vector3 nextdDist = socket.transform.position - axel.transform.position;
            if (nextdDist.magnitude < dist.magnitude) {
                dist = nextdDist;
                closest = socket;
            }
        }
        return closest;
    }

    private void parentToAxel() {
        if (connectedAxelSocket == null || isOnAxel()) {
            return;
        }
        TransformUtil.ParentToAndAlignXZ(transform, connectedAxelSocket.axel.transform, connectedAxelSocket.transform);
    }

    private void detachFromAxel() {
        if(isOnAxel()) {
            connectedAxelSocket.axel.occupied = false; 
        }
        transform.SetParent(null);
        connectedAxelSocket = null;
        
    }
	
	void Update () {
        updateAngleStep();
        //if (isOnAxel()) {
            foreach (Drivable dr in drivables) {
                dr.receiveDrive(new Drive(_angleStep.deltaAngle * radius));
            }
        //} 
	}

    //private void turnWithAxel() {
    //    transform.RotateAround(connectedAxelSocket.transform.position, orientation, connectedAxelSocket.axel.axisRotation);
    //}

    //public void turnByDegrees(float d) { 
    //    transform.Rotate(Vector3.up * d * Time.deltaTime);
    //}

    void OnTriggerEnter(Collider other) {
        if (isCursorInteracting) {
            if (!colliders.Contains(other)) {
                colliders.Add(other);
            }
        } else {
            //connectTo(other);
        }
        //TODO: want drivables also to add/be added if they move/rotate into other drivables when !isCursorInteracting (and remove/be removed)
    }

    void OnTriggerExit(Collider other) {
        colliders.Remove(other);
        print("on t exit " + other.name);
        Gear gear = other.GetComponent<Gear>(); 
        if (gear != null) {
            removeDrivable(gear);
        }
    }

    private bool connectTo(Collider other) {
        //TODO: consider swapping roles. The entering collider 
        if (isDrivenFromAxel()) {
            //if (other.GetComponent<Axel>())
                return false;
        }
        // If this is an axel, get driven by it
        Axel axel = getAxel(other);
        if (axel != null) {
            setSocket(axel);
            return true;
        }
        if (!isInConnectionRange(other)) {
            print("not in range");
            return false;
        }
        // If this is a gear, get driven by it
        Gear gear = other.GetComponent<Gear>(); // TODO: abstract 'Drivable' class instead of interface?
        if (gear != null && gear is Drivable) {
            _driver = gear;
            gear.addDrivable(this);
            positionRelativeTo(gear);
            return true;
        }
        return false;
    }

    public void addDrivable(Drivable _drivable) {
        if (!drivables.Contains(_drivable))
            drivables.Add(_drivable);
    }

    public void removeDrivable(Drivable _drivable) {
        while(drivables.Contains(_drivable)) {
            drivables.Remove(_drivable);
            _drivable.disconnectFromDriver();
        }
        print(drivables.Count);
    }

    private Axel getAxel(Collider other) {
        Axel anAxel = other.gameObject.GetComponent<Axel>();
        if (anAxel == null) {
            anAxel = other.gameObject.GetComponentInParent<Axel>();
        }
        if (anAxel != null) {
            if (anAxel.occupied) return null;
        }
        return anAxel;
    }

    private void setSocket(Axel anAxel) {
        if (anAxel != null) {
            if (isCursorInteracting) {
                //TODO: tell axel to highlight itself?
            } else {
                setSocketClosestToAxel(anAxel);
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
        isCursorInteracting = false;
        connectToColliders();
    }

    private void connectToColliders() {
        while(colliders.Count > 0) {
            Collider c = colliders[0];
            colliders.RemoveAt(0);
            if (connectTo(c)) {
                colliders.RemoveRange(0, colliders.Count);
                return;
            }
        }
    }

//TODO: make this less 'hacky' when we make a parent Drivable class (replace interface)
    private bool isInConnectionRange(Collider other) {
        if (other == null) return false;
        VectorXZ globalXZ = new VectorXZ(other.transform.position);
        CapsuleCollider cc = other.GetComponent<CapsuleCollider>();
        if (cc != null) {
            float centerDistance = (globalXZ - new VectorXZ(transform.position)).vector2.magnitude;
            return centerDistance < cc.radius * other.transform.localScale.x + radius;
        }
        return false;
    }

    public override Drive receiveDrive(Drive drive) {
        transform.eulerAngles += new Vector3(0f, drive.amount * -1f / radius, 0f);
        return drive;
    }

    public override void positionRelativeTo(Drivable _driver) {
        Gear gear = _driver.getComponent<Gear>();
        if (gear != null) {
            Vector3 relPos = transform.position - gear.transform.position;
            relPos = relPos.normalized * (radius + gear.radius - .01f); // fudge a little to keep gear inside
            transform.position = gear.transform.position + relPos;
            return;
        }
    }

    public override void disconnectFromDriver() {
        _driver = null;
    }

    public override T getComponent<T>() {
        return GetComponent<T>();
    }
}

public struct Drive
{
    public Transform atPoint;
    public float amount;

    public Drive(Transform _atPoint, float _amount) {
        atPoint = _atPoint; amount = _amount;
    }

    public Drive(float _amount) {
        atPoint = null; amount = _amount;
    }
}

public abstract class Drivable : MonoBehaviour
{
    protected AxelSocket[] axelSockets;

    void Awake () {
        //TODO: generate sockets programmatically
        awake();
	}

    public virtual void awake() {
        axelSockets = GetComponentsInChildren<AxelSocket>();
    }

    public abstract bool isDrivenFromAxel();
    public abstract Drive receiveDrive(Drive drive);
    public abstract void positionRelativeTo(Drivable _driver);
    public abstract void disconnectFromDriver();
    public abstract T getComponent<T>();
    
}
