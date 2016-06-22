using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Motor : Drivable
{
    protected HandleSet handleSet;
    protected LeverLimits leverLimits;
    protected float leverMultiplier;
    protected float leverMax = 10f;
    protected Handle lever { get { return handleSet.handles[0]; } }
    protected OnOffIndicator onOffIndicator;
    protected Counter counter;

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

//TODO: add on connections (on off gear switch) can't be reconnected?
    protected override DrivableConnection getDrivableConnection(Collider other) {
        DrivableConnection dc = new DrivableConnection(this);
        dc = getAddOnDrivableConnection(other, dc);
        print("conn was viable ? " + dc.viable);
        return dc;
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
            setLeverPositon((int)leverMultiplier);
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
        leverLimits.increments = 9;
        onOffIndicator = GetComponentInChildren<OnOffIndicator>();
        counter = GetComponentInChildren<Counter>();
        setLeverPositon((int)leverMultiplier);
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
    protected override void resetAddOnScalar() {
        power = 1f;
    }

    protected override void vDragOverride(VectorXZ cursorGlobal) {
        Bug.assertNotNullPause(leverLimits.min);
        float z = Mathf.Clamp(cursorGlobal.z, leverLimits.min.z, leverLimits.max.z);
        float gradient = leverLimits.gradientPosition(cursorGlobal.z);
        int lev = leverLimits.closestLevel(gradient);
        setMultiplier(lev);
        counter.turnTo(lev);
        setLeverPositon(lev);
    }

    protected void setLeverPositon(int lev) {
        Vector3 pos = lever.transform.position;
        pos.z = leverLimits.globalZPositionForLevel(lev);
        lever.transform.position = pos;
    }

    protected void setMultiplier(int lev) {
        leverMultiplier = lev;
        updateAudio();
    }
}
