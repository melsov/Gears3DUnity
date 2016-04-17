using UnityEngine;
using System.Collections.Generic;

public class WhiteKeysScale : MonoBehaviour {

    public string resourcePath = "Prefabs/Audio/Chimes/WhiteKeys";
    public string namePrefix = "WKXYLO";
    protected List<AudioEntity> notes = new List<AudioEntity>();

	void Awake () {
        AudioEntity[] anotes = Resources.LoadAll<AudioEntity>(resourcePath);
        int i = 0, totalNotes = 8;
        while (i < totalNotes) {
            foreach (AudioEntity ae in anotes) {
                if (ae.name.StartsWith("" + i) || ae.name.EndsWith("" + i)) {
                    notes.Add(ae);
                    ae.name = noteName(i);
                    break;
                }
            }
            i++;
        }
	}

    public string noteName(int i) {
        return namePrefix + i;
    }

    public AudioEntity getNote(string s) {
        try {
            return getNote(int.Parse(s));
        } catch (System.FormatException fe) {
            Debug.Log(fe);
        }
        return null;
    }

    public AudioEntity getNote(int i) {
        return notes[i % notes.Count];
    }

    public List<AudioEntity> getNotes() { return notes; }

}
