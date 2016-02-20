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
        SaveLoad.Instance.load();
    }

    public void newPressed() {
        SaveLoad.Instance.newScene();
    }
}
