using UnityEngine;
using System.Collections;

public class ObservableFloat {

    private float _storage;
    public float Value {
        get { return _storage; }
        set {
            float next = Mathf.Round(value * Mathf.Pow(10f, round)) / Mathf.Pow(10f, round);
            if (next != _storage) {
                onValueChange(_storage);
            }
            _storage = next;
        }
    }

    private readonly int round;

    public delegate void OnValueChange(float f);
    private OnValueChange onValueChange;

    public void register(OnValueChange onValueChange) {
        this.onValueChange += onValueChange;
    }

    public void unregister(OnValueChange onValueChange) {
        this.onValueChange -= onValueChange;
    }

    public ObservableFloat() : this(0f, 2) { }

    public ObservableFloat(int round) : this(0f, round) { }

    public ObservableFloat(float f, int round) {
        _storage = f;
        this.round = round;
    }
    
    public static implicit operator float(ObservableFloat obf) { return obf._storage; }
	
}
