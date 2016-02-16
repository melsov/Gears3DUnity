using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class ItemID : MonoBehaviour {
    public string itemID;
    private object _id;
    public int id {
        get {
            if (_id == null) {
                if (itemID == null || string.IsNullOrEmpty(itemID)) {
                    itemID = generateIDFromComponents();
                }
                _id = itemID.GetHashCode();
            }
            return (int)_id;
        }
    }

    public void updateIDWith(string _itemID) {
        _id = null;
        itemID = _itemID;
    }

    //Convenience: Not reliable for release version?
    private string generateIDFromComponents() {
        string result = "";
        GameObject go = gameObject;
        if (go.GetComponent<Cog>() == null) {
            foreach(Cog ch in GetComponentsInChildren<Cog>()) {
                go = ch.gameObject;
                break;
            }
        }

        foreach(MonoBehaviour mb in go.GetComponents<MonoBehaviour>()) {
            string mbName = mb.GetType().ToString();
            if (mbName.Length > 4) {
                mbName = mbName.Substring(0, 4);
            }
            result += mbName; 
        }
        Assert.IsTrue(result != "");
        print(result);
        return result;
    }
}
