using UnityEngine;
using System.Collections;
using System;

public class Chime : Cog , ICollisionProxyClient {

    //TODO: Audio source manager singleton. Else there's a risk of adding too many audio listeners?
    //Singleton should reject sounds if too many are playing

    //protected AudioSource _audioSource;
    protected WhiteKeysScale whiteKeyScale;
    [Range(0,8)]
    public int note = 0;

    void Awake () {
        whiteKeyScale = FindObjectOfType<WhiteKeysScale>();
        //_audioSource = GetComponent<AudioSource>();
	}

    public void proxyCollisionEnter(Collision collision) {
        AudioEntity ae = AudioManager.Instance.audioEntityFor(this, whiteKeyScale.noteName(note));
        if (!ae.getAudioSource().isPlaying || ae.getAudioSource().time > .9f) {
            AudioManager.Instance.play(this, whiteKeyScale.noteName(note));
        }
    }

    public void proxyCollisionExit(Collision collision) {
    }

    public void proxyCollisionStay(Collision collision) {
    }
}
