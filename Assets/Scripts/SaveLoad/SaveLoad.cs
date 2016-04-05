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

    protected void blockCursorInput(bool block) {
        //Camera.main.GetComponent<CursorInput>().blocked = block;
    }

    private List<Cog> allCogsInScene() {
        List<Cog> result = new List<Cog>();
        foreach (GameObject ga in GameObject.FindObjectsOfType<GameObject>()) {
            if (ga.activeInHierarchy && ga.GetComponent<Cog>() != null) {
                result.Add(ga.GetComponent<Cog>());
            }
        }
        return result;
    }

    public void save() {
        blockCursorInput(true);
        string doesntmatter = Application.dataPath + "/TempSaveFile";
        print(Application.persistentDataPath);
        Browser.Instance.SaveFile(doesntmatter, Application.persistentDataPath, handleSaveFile);
    }

    private void handleSaveFile(string filename) {
        SaveManager.Instance.SaveGame(filename);
        blockCursorInput(false);
    }

    public void load() {
        Browser.Instance.OpenFile(Application.persistentDataPath, handleLoadFile); //WANT
    }

    private void handleLoadFile(string filename) {
        newScene();
        SaveManager.Instance.LoadGame(filename);
        blockCursorInput(false);
    }
    

    public void newScene() {
        foreach(Cog cog in allCogsInScene()) {
            Destroy(cog.gameObject);
        }
    }
    
}
