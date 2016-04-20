using UnityEngine;
using System.Collections;

public class EmitterDevice : Dispenser {

    public Transform emitter;
    public float emissionTimeSeconds = 1f;
    [SerializeField]
    protected string soundName = AudioLibrary.WhooshSoundName;

    protected override void awake() {
        base.awake();
        emitter.gameObject.SetActive(false);
    }
    protected override void dispense() {
        StartCoroutine(emit());
    }

    private IEnumerator emit() {
        emitter.gameObject.SetActive(true);
        AudioManager.Instance.play(this, soundName);
        yield return new WaitForSeconds(emissionTimeSeconds);
        emitter.gameObject.SetActive(false);
    }
}
