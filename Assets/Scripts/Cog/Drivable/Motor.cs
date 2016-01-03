using UnityEngine;
using System.Collections;
using System;

public class Motor : Drivable
{
    public float maxAngularVelocity = 10f;
    protected float _power = 1f;
    public virtual float power {
        get { return _power; }
        set { _power = Mathf.Clamp(value, -1f, 1f); }
    }
    //public bool isPowered { get { return _power > 0f; } }

    protected Axel _axel;
    public Axel axel {
        get { return _axel;  }
    }

    protected float angle;
    
	protected override void awake () {
        base.awake();
        _axel = GetComponentInChildren<Axel>();
        UnityEngine.Assertions.Assert.IsTrue(_pegboard.getFrontendSocketSet().sockets.Length == 1);
        axel.beChildOf(_pegboard.getFrontendSocketSet().sockets[0]);
	}

	protected override void update () {
        //if (!isPowered) {
        //    return;
        //}
        angle += maxAngularVelocity * Time.deltaTime * power; // driveScalar();
        axel.turnTo(angle);
	}

    public override bool isDriven() {
        return true;
    }

    public override float driveScalar() {
        return 0; 
    }

    public override Drive receiveDrive(Drive drive) {
        return new Drive(0);
    }

    protected override void handleAddOnScalar(float scalar) {
        power = scalar;
    }

}
