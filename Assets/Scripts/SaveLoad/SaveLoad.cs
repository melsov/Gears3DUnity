using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Diagnostics;

public class SaveLoad : Singleton<SaveLoad> {

    private string folder = Application.persistentDataPath + "/";
    private string _filename;
    public string filename {
        get {
            if (_filename == null) {
                _filename = "Cog-temp.cog"; // + (System.DateTime.UtcNow.ToString()) + ".cog";
            }
            return _filename;
        }
        set { _filename = value; }
    }

    private List<Cog> sceneObjects() {
        List<Cog> result = new List<Cog>();
        foreach (GameObject ga in GameObject.FindObjectsOfType<GameObject>()) {
            if (ga.activeInHierarchy && ga.GetComponent<Cog>() != null) {
                result.Add(ga.GetComponent<Cog>());
            }
        }
        return result;
    }

    public void save() {
        //BinaryFormatter bf = new BinaryFormatter();
        //FileStream fs = File.Create(folder + filename);
        //bf.Serialize(fs, sceneObjects());
        //fs.Close();
        SaveManager.Instance.SaveGame("test1");
    }

    public void load(string filename_) {
        //string path = folder + filename_;
        //if (File.Exists(path)) {
        //    BinaryFormatter bf = new BinaryFormatter();
        //    FileStream fs = File.Open(path, FileMode.Open);
        //    List<GameObject> gas = (List<GameObject>)bf.Deserialize(fs);
        //    foreach(GameObject ga in gas) {
        //        print(ga.name);
        //    }
        //    print("deserialized");
        //    fs.Close();
        //}
        SaveManager.Instance.LoadGame("test1");
    }

    public void newScene() {
        foreach(Cog cog in sceneObjects()) {
            Destroy(cog.gameObject);
        }
    }
    
}
