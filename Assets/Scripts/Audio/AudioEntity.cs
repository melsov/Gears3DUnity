using UnityEngine;
using System.Collections;

public class AudioEntity : MonoBehaviour {

    public string name;
    protected AudioSource audioSource;

	void Awake () {
        audioSource = GetComponent<AudioSource>();
        Bug.assertNotNullPause(audioSource);
	}

    public AudioSource getAudioSource() {
        return audioSource;
    }

}
