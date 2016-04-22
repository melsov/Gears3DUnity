using UnityEngine;
using System.Collections.Generic;

public class AudioLibrary : Singleton<AudioLibrary>
{

    public static string GearSoundName = "GearAudioEntity";
    public static string TubeEnterSoundName = "TubeAudioEntity";
    public static string WhooshSoundName = "WhooshAudioEntity";
    public static string WaterfallSoundName = "WaterfallAudioEntity";

    private WhiteKeysScale whiteKeyScale;

    private Dictionary<string, AudioEntity> lookup; 

    private Dictionary<string, AudioEntity> getLookup() {
        if (lookup == null) {
            lookup = new Dictionary<string, AudioEntity>();
            whiteKeyScale = FindObjectOfType<WhiteKeysScale>();
            for (int i = 0; i < whiteKeyScale.getNotes().Count; ++i) {
                AudioEntity ae = whiteKeyScale.getNotes()[i];
                lookup.Add(whiteKeyScale.noteName(i), ae);
            }
            AudioEntity[] aes = Resources.LoadAll<AudioEntity>("Prefabs/Audio/Cog");
            foreach (AudioEntity ae in aes) {
                print(ae.name);
                lookup.Add(ae.name, ae);
            }
        }
        return lookup;
    }

    public AudioEntity getAudioEntity(string name) {
        try {
            return getLookup()[name];
        } catch (KeyNotFoundException knfe) {
            Debug.LogError("'" + name + "' wasn't found. " + knfe.StackTrace);
        }
        return null;
    }

}
