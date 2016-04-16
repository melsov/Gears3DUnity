using UnityEngine;
using System.Collections.Generic;

public class AudioLibrary : Singleton<AudioLibrary> {

    public static string GearSoundName = "Gear";
    public static string TubeEnterSoundName = "TubeEnter";

    private Dictionary<string, AudioEntity> lookup = new Dictionary<string, AudioEntity>();

    void Awake () {
        AudioEntity[] aes = Resources.LoadAll<AudioEntity>("Prefabs/Audio");
        foreach(AudioEntity ae in aes) {
            lookup.Add(ae.name, ae);
        }
	} 

    public AudioEntity getAudioEntity(string name) {
        return lookup[name];
    }

}
