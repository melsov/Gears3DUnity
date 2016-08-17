using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Runtime.Serialization;
using UnityEngine.SceneManagement;

public class SaveManager : Singleton<SaveManager> {
	protected SaveData mSaveData = new SaveData ();
	protected static string mSaveDirectory = "/save_games";
	protected bool mLoadGameAfterLevelLoaded = false;
	protected HashSet<GameObject> mDisabledGameObjects = new HashSet<GameObject> ();
	protected HashSet<GameObject> mDontDestroyOnLoadObjects = new HashSet<GameObject>();
	
	public string SaveDirectoryName
	{
		get
		{
			return Application.persistentDataPath + mSaveDirectory;
		}
	}
	
	protected string GetSaveFileName(string name)
	{
		string dirPath = Application.persistentDataPath + mSaveDirectory;
		string fileName = "/";
		fileName += name;
		fileName += ".sav";
		
		return dirPath + fileName;
	}
	
	public void SaveGame(string saveName)
	{
		if(!Directory.Exists(SaveDirectoryName))
		{
			Directory.CreateDirectory(SaveDirectoryName);
		}
		
		FillObjectsToSave ();

        string saveFileName = saveName; // GetSaveFileName(saveName);
		
		Debug.Log ("Saving game to: " + saveFileName);
		
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create (saveFileName);
		bf.Serialize(file, mSaveData);
		file.Close(); 
	}
	
	public bool LoadGame(string saveName)
	{
        string loadFileName = saveName; // GetSaveFileName(saveName);
		
		if(File.Exists(loadFileName))
		{
			Debug.Log ("Loading game from: " + loadFileName);
			
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(loadFileName, FileMode.Open);
			mSaveData = bf.Deserialize(file) as SaveData;
			file.Close();

            //mLoadGameAfterLevelLoaded = true;
            //Application.LoadLevel (mSaveData.levelName);

            //TODO: deal with loading on start up?
            LoadObjectsFromSave();
			
			return true;
		}
		else
		{
			Debug.Log ("Cannot load game, file does not exists: " + loadFileName);
		}
		
		return false;
	}
	
	// Called when level is loaded by Unity.
	//void OnLevelWasLoaded(int level)
	//{
	//	if( mLoadGameAfterLevelLoaded )
	//	{
	//		mLoadGameAfterLevelLoaded = false;
	//		LoadObjectsFromSave();
	//	}
	//}
	
	protected void FillObjectsToSave()
	{
        mSaveData.levelName = SceneManager.GetActiveScene().name; // Application.loadedLevelName;
		mSaveData.entries.Clear ();
		
		//SetDisabledGameObjectsActive (true);
		GameObject[] objectsInScene = GameObject.FindObjectsOfType (typeof(GameObject)) as GameObject[];
        //SetDisabledGameObjectsActive (false);

        // MMP: prefabs instances may have the same guids. So always update them before saving.
        // Having new guids per each save isn't a problem since the kind of prefab is recovered using its itemID.
		foreach (GameObject go in objectsInScene)
		{
            if ( (go.GetComponent("Guid") as Guid) != null && go.GetComponent<ItemID>() != null )
			{
                go.GetComponent<Guid>().Generate(); 
			}
		}

		foreach (GameObject go in objectsInScene)
		{
            if ( (go.GetComponent("Guid") as Guid) != null && go.GetComponent<ItemID>() != null )
			{
                mSaveData.entries.Add(new SaveEntry(go));
			}
		}
	}
	
	protected void LoadObjectsFromSave()
	{
        foreach (SaveEntry en in mSaveData.entries) {
            en.RestoreGameObject();
        }

        //Reinstate connections
        foreach (SaveEntry en in mSaveData.entries) {
            en.RestoreConnections();
        }

        foreach(GameObject go in GameObject.FindObjectsOfType<GameObject>()) {
            if (go.GetComponent<Guid>() != null) {
                foreach (IConstrainable constrainable in go.GetComponentsInChildren<IConstrainable>()) {
                    constrainable.setupConstraint();
                }
            }
        }
	}

//	protected void LoadObjectsFromSaveOLD()
//	{
//		List<GameObject> loadedGameObjects = new List<GameObject> ();
		
//		//SetDisabledGameObjectsActive (true);
//		GameObject [] objectsInScene = GameObject.FindObjectsOfType (typeof(GameObject)) as GameObject[];
//		//SetDisabledGameObjectsActive (false);

////		MultiMap<string, gameobject=""> guidToGO = new MultiMap<string, gameobject="">();
//		MultiMap<string, GameObject> guidToGO = new MultiMap<string, GameObject>();
		
//// CONSIDER: whole new approach: each object is reinstantiated from a prefab
//// A helper object is serialized/deserialized. Helper object tells which prefab is needed and has any additional data: alla SerializeStorage
//		foreach (GameObject go in objectsInScene)
//		{
//			Guid g = go.GetComponent("Guid") as Guid;
//			if (g != null)
//			{
//				guidToGO.Add(g.guid.ToString(), go);
//			}
//		}

//        foreach (SaveEntry en in mSaveData.entries) {
//            if (guidToGO.ContainsKey(en.guid)) {
//                //			HashSet gos = guidToGO.GetValues(en.guid, true);
//                List<GameObject> gos = guidToGO[en.guid];

//                if (gos.Count > 1) {
//                    throw new Exception("Multiple objects with same GUID found. Something's wrong.");
//                }

//                foreach (GameObject go in gos) {
//                    en.RestoreGameObject(go);
//                    loadedGameObjects.Add(go);
//                }
//            } else {
//                throw new Exception("Saved game does not correspond to the contents of the level!");
//            }
//        }
		
//		// delete all in-scene, but not loaded gameobjects (they were deleted during gameplay)
//		foreach(GameObject go in objectsInScene)
//		{
//			Guid g = go.GetComponent("Guid") as Guid;
			
//			if( g != null && !loadedGameObjects.Contains(go) && !mDontDestroyOnLoadObjects.Contains(go) )
//			{
//				Destroy (go);
//			}
//		}
//	}
	
	public void SerializeIntoArray(System.Object storage, ref List<byte[]> data)
	{
		IFormatter formatter = new BinaryFormatter();
		using (MemoryStream stream = new MemoryStream())
		{
			formatter.Serialize(stream, storage);
			byte [] bytes = stream.ToArray();
			data.Add (bytes);
		}
	}
	
	public T DeserializeFromArray<T>(ref List<byte[]> array)
	{
		if( array.Count > 0 )
		{
			BinaryFormatter bf = new BinaryFormatter();
			MemoryStream ms = new MemoryStream (array[0]);
			T storage = (T)bf.Deserialize (ms);
			array.RemoveAt (0);
			return storage;
		}
		
		return default(T);
	}
	
	public GameObject FindGameObjectByGuid(string guid)
	{
        if (guid == null) return null;
		foreach (GameObject go in GameObject.FindObjectsOfType(typeof (GameObject)))
		{
			Guid g = go.GetComponent("Guid") as Guid;
			if( g != null )
			{
				if( g.guid.ToString() == guid )
				{
					return go;
				}
			}
		}
		
		return null;
	}
}

public interface IRestoreConnection
{
    void storeConnectionData(ref List<byte[]> connectionData);
    void restoreConnectionData(ref List<byte[]> connectionData);
}

public interface IConstrainable
{
    void setupConstraint();
}