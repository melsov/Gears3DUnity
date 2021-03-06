﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public interface IGameSerializable
{
	void Serialize (ref List<byte[]> data);
	void Deserialize(ref List<byte[]> data);
}

[System.Serializable] 
public class SaveEntry
{
	public SaveEntry(GameObject obj)
	{
		StoreGameobject (obj);
	}
	
	public string guid = null;
	public bool active;
	public SerializableVector3 position;
	public SerializableVector3 localScale;
	public SerializableQuaternion rotation;
	public List<byte[]> scriptData = new List<byte[]>();	// custom classes serialize into this array
    public List<byte[]> connectionData = new List<byte[]>(); // custom connection data
    public int itemID;

    [System.NonSerialized]
    private GameObject reinstantiatedGameObject;
    private void reinstantiateGameObject() {
        Transform t = Inventory.Instance.prefabWithId(itemID);
        if (t == null) { throw new Exception("no prefab for entry: " + guid);  }
        Transform instance = UnityEngine.Object.Instantiate(t);
        reinstantiatedGameObject = instance.gameObject;
    }

    public void StoreGameobject(GameObject obj)
	{
		if (obj.GetComponent<Guid>()) {
			guid = (obj.GetComponent ("Guid") as Guid).guid.ToString ();
		}

        //MMP
        if (obj.GetComponent<ItemID>()) {
            itemID = obj.GetComponent<ItemID>().id;
        } else { throw new System.Exception("no itemID componenent for: " + obj.name); }


        position = obj.transform.position;
		localScale = obj.transform.localScale;
		rotation = obj.transform.rotation;
		active = obj.activeSelf;
		
		// serialize IGameSerializable
        foreach(IGameSerializable ser in obj.GetComponents<IGameSerializable>()) {
            ser.Serialize(ref scriptData);
        }

        //Save connection data
        Manifest manifest = obj.GetComponent<Manifest>();
        if (manifest == null) { Debug.LogError("this object needs a manifest: " + obj.name); return; }

        foreach(MonoBehaviour mb in manifest.prefabComponents) {
            if (mb is IRestoreConnection) {
                ((IRestoreConnection)mb).storeConnectionData(ref connectionData);
            }
        }


	}

	
	public void RestoreGameObject()
	{
        if (reinstantiatedGameObject == null) {
            reinstantiateGameObject();
        }
		reinstantiatedGameObject.transform.position = position;
		//reinstantiatedGameObject.transform.localScale = localScale;//Don't restore scale
		reinstantiatedGameObject.transform.rotation = rotation;
		reinstantiatedGameObject.SetActive (active);
        reinstantiatedGameObject.GetComponent<Guid>().guid = new System.Guid(guid);
		
		// deserialize custom classes
        foreach(IGameSerializable ser in reinstantiatedGameObject.GetComponents<IGameSerializable>()) {
            ser.Deserialize(ref scriptData);
        }
	}

    public void RestoreConnections() {
        if (reinstantiatedGameObject == null) { throw new Exception("game object not reinstantiated yet"); }
        // CONSIDER: unpredictable order of this enumeration causes errors possibly?
        // Is it possible to make connectionData be a dictionary? 
        foreach (IRestoreConnection rc in reinstantiatedGameObject.GetComponentsInChildren<IRestoreConnection>()) {
            rc.restoreConnectionData(ref connectionData);
        }
    }
}
