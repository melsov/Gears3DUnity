using UnityEngine;
using System.Collections;

public class SharedPrefabs : Singleton<SharedPrefabs> {

    protected SharedPrefabs() { }

    private static string pegPath = "Prefabs/Component/DecorativePeg";
    private Transform _peg;
    private Transform peg {
        get {
            if (!_peg) {
                _peg = Instantiate(Resources.Load<Transform>(pegPath));
                _peg.gameObject.SetActive(false);
            }
            return _peg;
        }
    }

    public Transform createPeg() {
        Transform s = Instantiate(peg);
        s.gameObject.SetActive(true);
        return s;
    }
}
