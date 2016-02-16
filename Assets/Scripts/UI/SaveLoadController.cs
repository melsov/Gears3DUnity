using UnityEngine;
using System.Collections;

public class SaveLoadController : MonoBehaviour {

    public void savePressed() {
        SaveLoad.Instance.save();
    }

    public void openPressed() {
        load();
    }

    private void load() {
        string temp_file = SaveLoad.Instance.filename;
        SaveLoad.Instance.load(temp_file);
    }

    public void newPressed() {
        SaveLoad.Instance.newScene();
    }
}
