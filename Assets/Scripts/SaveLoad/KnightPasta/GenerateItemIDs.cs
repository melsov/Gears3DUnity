using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;


public class GenerateItemIDs : ScriptableWizard {

    [MenuItem("Custom/Re-generate ItemIDs")]
    static void Generate() {
        ItemID[] prefabs = Resources.LoadAll<ItemID>("Prefabs/Cog");
        HashSet<int> ids = new HashSet<int>();
        Debug.Log("hi " + prefabs.Length + ".");
        foreach(ItemID itemID in prefabs) {
            int increment = 1;
            while(ids.Contains(itemID.id)) {
                int index = 1;
                if (increment == 1) {
                    index = 0;
                }
                string idstr = "" + (increment++) + itemID.itemID.Substring(index);
                itemID.updateIDWith(idstr);
            }
            Debug.LogError(itemID.id);
            UnityEngine.Assertions.Assert.IsTrue(!ids.Contains(itemID.id));
            ids.Add(itemID.id);
            GUI.changed = true;
            EditorUtility.SetDirty( itemID.gameObject);
        }
    }

    [MenuItem("Custom/Add Gui ItemIDs Manifests GenerateGuids")]
    static void AddGuidAndItemId() {
        MonoBehaviour[] prefabs = Resources.LoadAll<MonoBehaviour>("Prefabs/Cog");
        foreach(MonoBehaviour mb in prefabs) {
            if (mb.GetComponent<ItemID>() == null) {
                mb.gameObject.AddComponent<ItemID>();
            }
            Guid guid = mb.GetComponent<Guid>(); // null;
            if (guid == null) {
                guid = mb.gameObject.AddComponent<Guid>();
            }
            guid.Generate();
            if (mb.GetComponent<Manifest>() == null) {
                mb.gameObject.AddComponent<Manifest>();
            }
            GUI.changed = true;
            EditorUtility.SetDirty(mb.gameObject);
        }
    }


    //[MenuItem("Custom/Add Manifests to Prefabs")]
    //static void AddManifestToPrefabs() {
    //    MonoBehaviour[] prefabs = Resources.LoadAll<MonoBehaviour>("Prefabs/Cog");
    //    foreach(MonoBehaviour mb in prefabs) {
    //        GUI.changed = true;
    //        EditorUtility.SetDirty(mb.gameObject);
    //    }
    //}

    [MenuItem("Custom/Test Guid")]
    static void TestGuid() {
        System.Guid gu = System.Guid.NewGuid();
        Debug.Log(gu);
        System.Guid other = new System.Guid(gu.ToString());
        Debug.Log(other);
    }

}
#else
public class GenerateItemIDs {}
#endif
