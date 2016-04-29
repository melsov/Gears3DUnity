using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;


public class ScoreManager : Singleton<ScoreManager> {

    private uint _score;
    public uint score {
        get { return _score; }
    }
    public Text scoreText;

    public void notify(Scorable scorable) {
        _score += scorable.value;
        scoreText.text = "" + _score;
    }

}

