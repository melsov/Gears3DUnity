using UnityEngine;
using System.Collections;
using System;

public class Percussion : Instrument {

    [SerializeField]
    protected string audioEntityName;
    [SerializeField]
    protected Color highlight = Color.green;

    protected override Color getColor() {
        return highlight;
    }

    protected override string getNoteName() {
        return audioEntityName;
    }

}
