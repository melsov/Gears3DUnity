using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class AudioManager : Singleton<AudioManager> {
    // TODO: know which (note) sounds are playing when (not nec. in AudioManager?)

    public uint maxAllowedAudioSources = 16; //a guess
    List<Cog> sources = new List<Cog>();

    public void play(Cog cog, string soundName) {
        getAudioEntityFor(cog, soundName).getAudioSource().Play();
    }

    public void stop(Cog cog, string soundName) {
        AudioEntity ae = lookupAudioEntityFor(cog, soundName);
        if (ae != null) {
            ae.getAudioSource().Stop();
        }
    }

//CONSIDER: is this too slow (using GetCIC<> everytime)?
    public AudioEntity getAudioEntityFor(Cog cog, string soundName) {
        AudioEntity ae = lookupAudioEntityFor(cog, soundName);
        if (ae != null) { return ae; }
        return attachAudioEntity(cog, soundName);
    }

    public AudioEntity lookupAudioEntityFor(Cog cog, string soundName) {
        foreach (AudioEntity ae in cog.GetComponentsInChildren<AudioEntity>()) {
            string n = ae.name;
            string clone = "(Clone)";
            if (ae.name.EndsWith(clone)) {
                n = n.Substring(0, n.Length - clone.Length);
            }
            if (n.Equals(soundName)) { return ae; }
        }
        return null;
    }

    private AudioEntity attachAudioEntity(Cog cog, string soundName) {
        cullIfLimit();
        AudioEntity ae = Instantiate(AudioLibrary.Instance.getAudioEntity(soundName));
        ae.transform.position = cog.transform.position;
        ae.transform.parent = cog.transform;
        
        if (!sources.Contains(cog)) {
            sources.Add(cog);
        }
        return ae;
    }

    private void cullIfLimit() {
        if (sources.Count <= maxAllowedAudioSources) {
            return;
        }
        AudioEntity ae = null;
        AudioEntity[] entities = null;
        Cog cog = null;

        for(int i = 0; i < sources.Count; ++i) {
            cog = sources[i];
            if (cog == null) { sources.RemoveAt(i--); continue; }
            entities = cog.GetComponentsInChildren<AudioEntity>();
            if (entities == null || entities.Length == 0) {
                sources.RemoveAt(i--);
                continue;
            }
            ae = entities[0];
            break;
        }
        Destroy(ae.gameObject);
        if (cog == null || cog.GetComponentsInChildren<AudioEntity>().Length == 0) {
            sources.Remove(cog);
        }
        
    }

}



