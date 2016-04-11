using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class AudioManager : Singleton<AudioManager> {

    protected HashSet<AudioSource> sources = new HashSet<AudioSource>();
    public SoundType[] soundTypes = new SoundType[] { };
    //protected Dictionary<Type, AudioSource> lookup = new Dictionary<Type, AudioSource>();

    public void play(Cog cog, bool repeat) {
        AudioSource aus = sourceFor(cog);
        if (aus == null) { return; }
        aus.loop = repeat;
        aus.Play();
    }

    private AudioSource sourceFor(Cog cog) {
        foreach(SoundType soundT in soundTypes) {
            if (cog.GetType() == soundT.cog.GetType()) {
                return soundT.audioSource;
            }
        }
        return null;
    }
}

[Serializable]
public struct SoundType
{
    public Cog cog;
    public AudioSource audioSource;
}
