using UnityEngine;
using System.Collections;
using System;

public class Chime : MonoBehaviour , ICollisionProxyClient {

    protected AudioSource _audioSource;

    public void proxyCollisionEnter(Collision collision) {
        if (!_audioSource.isPlaying || _audioSource.time > .03f) {
            _audioSource.Play();
        }
    }

    public void proxyCollisionExit(Collision collision) {
    }

    public void proxyCollisionStay(Collision collision) {
    }

    void Awake () {
        _audioSource = GetComponent<AudioSource>();
	}

    //void OnCollisionEnter(Collision other) {
        
    //}
}
