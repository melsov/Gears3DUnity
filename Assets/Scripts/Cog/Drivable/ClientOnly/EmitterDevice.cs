using UnityEngine;
using System.Collections;
using System;

public class EmitterDevice : Dispenser {

    public Transform emitter;
    public float emissionTimeSeconds = 1f;
    [SerializeField]
    protected string soundName = AudioLibrary.WhooshSoundName;
    public bool toggleOnOff = false;
    OpenCloseAnimationHandler openCloseAnimationHandler;

    protected override void awake() {
        base.awake();
        emitter.gameObject.SetActive(false);
        openCloseAnimationHandler = GetComponentInChildren<OpenCloseAnimationHandler>();
        if (openCloseAnimationHandler != null) {
            openCloseAnimationHandler.stateChangedTo = animatorCallback;
        }
    }

    protected override float power {
        get {
            return base.power;
        }
        set {
            if (Time.fixedTime - timer > fireRate) {
                if (toggleOnOff) {
                    emit(value > 0f);
                    //shouldDispense = true;
                    //_power = 1f;
                    timer = Time.fixedTime;
                } else {
                    shouldDispense = true;

                }
            }
        }
    }

    protected override void dispense() {
        if (toggleOnOff) {
            emit(!emitter.gameObject.activeSelf);
            return;
        }
        StartCoroutine(pulseEmit());
    }

    private IEnumerator pulseEmit() {
        emit(true);
        //AudioManager.Instance.play(this, soundName);
        yield return new WaitForSeconds(emissionTimeSeconds);
        emit(false);
    }

    private void emit(bool _emit) {
        if (_emit == isEmitting) { return; }
        if (openCloseAnimationHandler != null) {
            openCloseAnimationHandler.open(_emit);
            return;
        }
        activate(_emit);
    }

    private void animatorCallback(bool state) {
        print("call back: " + state);
        activate(state);
    }

    private bool isEmitting { get { return emitter.gameObject.activeSelf; } }

    private void activate(bool state) {
        emitter.gameObject.SetActive(state);
        if (emitter.gameObject.activeSelf) {
            AudioManager.Instance.play(this, soundName);
        } else if (toggleOnOff) {
            AudioManager.Instance.stop(this, soundName);
        }
    }

  
}
