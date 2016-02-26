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
        //SaveManager.Instance.SaveGame("test1");

        //WANT
        string doesntmatter = Application.dataPath + "/TempSaveFile";
        print(Application.persistentDataPath);
        Browser.Instance.SaveFile(doesntmatter, Application.persistentDataPath, handleSaveFile);
    }

    private void handleSaveFile(string filename) {
        SaveManager.Instance.SaveGame(filename);
    }

    public void load() {
        //SaveManager.Instance.LoadGame("test1");
        Browser.Instance.OpenFile(Application.persistentDataPath, handleLoadFile); //WANT
    }

    private void handleLoadFile(string filename) {
        SaveManager.Instance.LoadGame(filename);
    }
    

    public void newScene() {
        foreach(Cog cog in sceneObjects()) {
            Destroy(cog.gameObject);
        }
    }
    
}
