﻿using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class GenerateItemIDs : ScriptableWizard {

    [MenuItem("Custom/Re-generate ItemIDs")]
    static void Generate() {
        ItemID[] prefabs = Resources.LoadAll<ItemID>("Prefabs/Cog");
        HashSet<int> ids = new HashSet<int>();
        foreach(ItemID itemID in prefabs) {
            int increment = 1;
            while(ids.Contains(itemID.id)) {
                int index = 1;
                if (increment == 1) {
                    index = 0;
                }
                itemID.updateIDWith("" + (increment++) + itemID.itemID.Substring(index));
            }
            ids.Add(itemID.id);
            GUI.changed = true;
            EditorUtility.SetDirty( itemID.gameObject );
        }
    }

    [MenuItem("Custom/Test Guid")]
    static void TestGuid() {
        System.Guid gu = System.Guid.NewGuid();
        Debug.Log(gu);
        System.Guid other = new System.Guid(gu.ToString());
        Debug.Log(other);
    }

}
