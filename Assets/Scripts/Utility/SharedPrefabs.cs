using UnityEngine;
using System.Collections;

public class SharedPrefabs : Singleton<SharedPrefabs> {

    protected SharedPrefabs() { }

    private static string socketPath = "Prefabs/Component/DecorativePeg";
    private Transform _socket;
    private Transform socket {
        get {
            if (!_socket) {
                _socket = Instantiate(Resources.Load<Transform>(socketPath));
                _socket.gameObject.SetActive(false);
            }
            return _socket;
        }
    }

    public Transform createSocket() {
        Transform s = Instantiate(socket);
        s.gameObject.SetActive(true);
        return s;
    }
}
