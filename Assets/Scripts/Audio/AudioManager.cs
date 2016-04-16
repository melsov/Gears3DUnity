using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class AudioManager : Singleton<AudioManager> {
    // TODO: know which (note) sounds are playing when (not nec. in AudioManager?)

    public uint maxAllowedAudioSources = 16; //a guess
    Dictionary<Cog, AudioEntity> sources = new Dictionary<Cog, AudioEntity>();

    public void play(Cog cog, string soundName) {
        AudioEntity ae = audioEntityFor(cog, soundName);
        if (ae == null) {
            ae = attachAudioEntity(cog, soundName);
        }
        ae.getAudioSource().Play();
    }

    private AudioEntity audioEntityFor(Cog cog, string soundName) {
        foreach (AudioEntity ae in cog.GetComponentsInChildren<AudioEntity>()) {
            if (ae.name.Equals(soundName)) { return ae; }
        }
        return null;
    }

    private AudioEntity attachAudioEntity(Cog cog, string soundName) {
        cullIfLimit();
        AudioEntity ae = Instantiate(AudioLibrary.Instance.getAudioEntity(soundName));
        ae.transform.position = cog.transform.position;
        ae.transform.parent = cog.transform;
        sources.Add(cog, ae);
        return ae;
    }

//CONSIDER: doesn't really do what we want: assumes one audio entity per cog...
    private void cullIfLimit() {
        if (sources.Count > maxAllowedAudioSources) {
            AudioEntity ae = null;
            Cog cog = null;
            foreach(Cog c in sources.Keys) {
                cog = c;
                ae = sources[cog];
                if (ae == null) {
                    sources.Remove(cog);
                    continue;
                }
                break;
            }
            sources.Remove(cog);
            Destroy(ae.gameObject);
        }
    }

    public void remove(Cog cog) {
        sources.Remove(cog);
    }


}



