using UnityEngine;
using System.Collections;

public class EmitterDevice : Dispenser {

    public Transform emitter;
    public float emissionTimeSeconds = 1f;
    [SerializeField]
    protected string soundName = AudioLibrary.WhooshSoundName;
    public bool toggleOnOff = false;

    protected override void awake() {
        base.awake();
        emitter.gameObject.SetActive(false);
    }
    protected override void dispense() {
        if (toggleOnOff) {
            emitter.gameObject.SetActive(!emitter.gameObject.activeSelf);
            if (emitter.gameObject.activeSelf) {
                AudioManager.Instance.play(this, soundName);
            } else {
                AudioManager.Instance.stop(this, soundName);
            }
            return;
        }
        StartCoroutine(emit());
    }

    private IEnumerator emit() {
        emitter.gameObject.SetActive(true);
        AudioManager.Instance.play(this, soundName);
        yield return new WaitForSeconds(emissionTimeSeconds);
        emitter.gameObject.SetActive(false);
    }
}
