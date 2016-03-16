using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Motor : Drivable // , IGameSerializable
{
    public float maxAngularVelocity = 10f;
    protected float _power = 1f;
    public virtual float power {
        get { return _power * _isPaused; }
        set { _power = Mathf.Clamp(value, -1f, 1f); }
    }
    protected float _isPaused = 1f;
    protected override void pause(bool isPaused) {
        base.pause(isPaused);
        _isPaused = isPaused ? 0f : 1f;
    }

    protected Axel _axel;
    public Axel axel {
        get { return _axel;  }
    }

    protected float angle;
    
    [System.Serializable]
    class SerializeStorage
    {
        public float maxAngularVelocity;
        public float _power;
    }
    public override void Serialize(ref List<byte[]> data) {
        base.Serialize(ref data);
        SerializeStorage stor = new SerializeStorage();
        stor.maxAngularVelocity = maxAngularVelocity;
        stor._power = _power;
        SaveManager.Instance.SerializeIntoArray(stor, ref data);
    }

    public override void Deserialize(ref List<byte[]> data) {
        base.Deserialize(ref data);
        SerializeStorage stor;
        if((stor = SaveManager.Instance.DeserializeFromArray<SerializeStorage>(ref data)) != null) {
            maxAngularVelocity = stor.maxAngularVelocity;
            _power = stor._power;
        }
    }

	protected override void awake () {
        base.awake();
        _axel = GetComponentInChildren<Axel>();
        UnityEngine.Assertions.Assert.IsTrue(_pegboard.getFrontendSocketSet().sockets.Length == 1);
        axel.beChildOf(_pegboard.getFrontendSocketSet().sockets[0]);
	}

	protected override void update () {
        angle += maxAngularVelocity * Time.deltaTime * power;
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
