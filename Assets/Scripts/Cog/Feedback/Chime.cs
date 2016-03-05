using UnityEngine;
using System.Collections;
using System;

public class Chime : Cog , ICollisionProxyClient {

//TODO: Audio source manager singleton. Else there's a risk of adding too many audio listeners?
//Singleton should reject sounds if too many are playing

    protected AudioSource _audioSource;

    void Awake () {
        _audioSource = GetComponent<AudioSource>();
	}

    public void proxyCollisionEnter(Collision collision) {
        if (!_audioSource.isPlaying || _audioSource.time > .9f) {
            _audioSource.Play();
        }
    }

    public void proxyCollisionExit(Collision collision) {
    }

    public void proxyCollisionStay(Collision collision) {
    }
}
