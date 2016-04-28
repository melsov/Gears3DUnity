using UnityEngine;
using System.Collections;

public class Scorable : MonoBehaviour {
    public uint value = 1;
    public string _name;

    public void Start() {
        ScoreManager.Instance.notify(this);
    }
}
