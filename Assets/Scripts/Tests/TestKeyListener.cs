using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class TestKeyListener : Singleton<TestKeyListener>
{
    protected TestKeyListener() { }

    public delegate void KeyDownAction();
    private Dictionary<KeyCode, KeyDownAction> keyCodeDown = new Dictionary<KeyCode, KeyDownAction>();

    public void addKeyCode(KeyCode kc, KeyDownAction kda) {
        if (!keyCodeDown.ContainsKey(kc)) {
            keyCodeDown.Add(kc, kda);
        } else {
            keyCodeDown[kc] += kda;
        }
    }

    public void Update() {
        foreach(KeyCode kc in keyCodeDown.Keys) {
            if (Input.GetKeyDown(kc)) {
                KeyDownAction kda = keyCodeDown[kc];
                if (kda != null) {
                    kda();
                }
            }
        }
    }
}

