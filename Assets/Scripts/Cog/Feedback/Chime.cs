using UnityEngine;
using System.Collections;
using System;

public class Chime : Cog , ICollisionProxyClient {

    //TODO: Audio source manager singleton. Else there's a risk of adding too many audio listeners?
    //Singleton should reject sounds if too many are playing

    //protected AudioSource _audioSource;
    protected WhiteKeysScale whiteKeyScale;
    public const int keyCount = 8;
    [Range(0,keyCount - 1)]
    public int note = 0;
    protected ColorRange colorRange = new ColorRange(keyCount - 1, Color.red);
    protected Highlighter highlighter;

    void Awake () {
        whiteKeyScale = FindObjectOfType<WhiteKeysScale>();
        highlighter = GetComponent<Highlighter>();
	}

    public void proxyCollisionEnter(Collision collision) {
        AudioEntity ae = AudioManager.Instance.audioEntityFor(this, whiteKeyScale.noteName(note));
        if (!ae.getAudioSource().isPlaying || ae.getAudioSource().time > .9f) {
            AudioManager.Instance.play(this, whiteKeyScale.noteName(note));
            highlighter.highlightForSeconds(.9f, colorRange[note]);
        }
    }


    public void proxyCollisionExit(Collision collision) {
        //highlighter.unhighlight();
    }

    public void proxyCollisionStay(Collision collision) {
    }
}
