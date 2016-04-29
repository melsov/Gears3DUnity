using UnityEngine;
using System.Collections;

public abstract class Instrument : Cog , ICollisionProxyClient {
    protected Highlighter highlighter;

    public void Awake () {
        awake();
	}

    protected virtual void awake() {
        highlighter = GetComponent<Highlighter>();
    }

    protected abstract string getNoteName();
    protected abstract Color getColor();
    [SerializeField]
    protected float repeatInterval = .9f;

    public void proxyCollisionEnter(Collision collision) {
        AudioEntity ae = AudioManager.Instance.getAudioEntityFor(this, getNoteName());
        if (!ae.getAudioSource().isPlaying || ae.getAudioSource().time > repeatInterval) {
            AudioManager.Instance.play(this, getNoteName());
            highlighter.highlightForSeconds(.9f, getColor());
        }
    }

    public void proxyCollisionExit(Collision collision) {
    }

    public void proxyCollisionStay(Collision collision) {
    } 

}
