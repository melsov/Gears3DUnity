using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;


public class UpdateChimes : ScriptableWizard {

    [MenuItem("Custom/UpdateChimes")]
    static void UpdateThem() {
        Debug.LogError("hi");
        //Chime[] one = Resources.LoadAll<Chime>("Prefabs/Cog/Chimes/C");
        Chime chimeC = null;
        Chime[] chimes = Resources.LoadAll<Chime>("Prefabs/Cog/Chimes");
        foreach(Chime ch in chimes) {
            if (ch.name == "ChimeC") {
                chimeC = ch; break;
            }
        }
        if (chimeC == null) {
            Debug.Log("chime c null"); return;
        }
        foreach(Chime ch in chimes) {
            if (ch == chimeC) { continue; }
            Debug.Log(ch.name);
            int note = ch.note;
            Chime replacement = Instantiate<Chime>(chimeC);
            replacement.note = note;
            PrefabUtility.ReplacePrefab(replacement.gameObject, ch, ReplacePrefabOptions.Default);
        }
        
    }
    

}
#else
public class UpdateChimes {}
#endif
