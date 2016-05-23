using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Motor : Drivable
{
    protected HandleSet handleSet;
    protected LeverLimits leverLimits;
    protected float leverMultiplier;
    protected float leverMax = 16f;
    protected Handle lever { get { return handleSet.handles[0]; } }
    protected OnOffIndicator onOffIndicator;

    public float maxAngularVelocity = 10f;
    protected float _power = 1f;
    public virtual float power {
        get { return _power * _isPaused * leverMultiplier; }
        set {
            _power = Mathf.Clamp(value, -1f, 1f);
            if(onOffIndicator != null) {
                onOffIndicator.state = SwitchStateHelper.stateFor(_power);
            }
            updateAudio();
        }
    }

    protected void updateAudio() {
        if (Angles.VerySmall(power)) { AudioManager.Instance.stop(this, AudioLibrary.GearSoundName); }
        else { AudioManager.Instance.play(this, AudioLibrary.GearSoundName); }
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

    #region save
    [System.Serializable]
    class SerializeStorage
    {
        public float maxAngularVelocity;
        public float _power;
        public float _leverMultiplier;
    }
    public override void Serialize(ref List<byte[]> data) {
        base.Serialize(ref data);
        SerializeStorage stor = new SerializeStorage();
        stor.maxAngularVelocity = maxAngularVelocity;
        stor._power = _power;
        stor._leverMultiplier = leverMultiplier;
        SaveManager.Instance.SerializeIntoArray(stor, ref data);
    }

    public override void Deserialize(ref List<byte[]> data) {
        base.Deserialize(ref data);
        SerializeStorage stor;
        if((stor = SaveManager.Instance.DeserializeFromArray<SerializeStorage>(ref data)) != null) {
            maxAngularVelocity = stor.maxAngularVelocity;
            _power = stor._power;
            leverMultiplier = stor._leverMultiplier;
            setLeverPositon();
        }
    }
    #endregion

    protected override void awake () {
        base.awake();
        _axel = GetComponentInChildren<Axel>();
        UnityEngine.Assertions.Assert.IsTrue(_pegboard.getFrontendSocketSet().sockets.Length == 1);
        axel.beChildOf(_pegboard.getFrontendSocketSet().sockets[0]);
        handleSet = GetComponentInChildren<HandleSet>();
        leverLimits = GetComponentInChildren<LeverLimits>();
        onOffIndicator = GetComponentInChildren<OnOffIndicator>();
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
        print("handle add on scalar: " + scalar);
        power = scalar;
    }
    protected override void resetAddOnScalar() {
        power = 1f;
    }

    protected override void vDragOverride(VectorXZ cursorGlobal) {
        //TODO: set a (separate?) scalar that influences motor power
        // and raise and lower the lever
        float z = Mathf.Clamp(cursorGlobal.z, leverLimits.min.z, leverLimits.max.z);
        Vector3 pos = lever.transform.position;
        pos.z = z;
        lever.transform.position = pos;
        setMultiplier();
        setLeverPositon();
    }

    protected void setLeverPositon() {
        Vector3 pos = lever.transform.position;
        pos.z = leverLimits.min.z + leverLimits.distance * (leverMultiplier / leverMax);
        lever.transform.position = pos;
    }

    protected void setMultiplier() {
        leverMultiplier = Mathf.Floor((leverMax + .5f) * (lever.transform.position.z - leverLimits.min.z) / leverLimits.distance);
        updateAudio();
    }
}
