using UnityEngine;
using System.Collections;
using System;

public class Chime : Instrument {
    protected WhiteKeysScale whiteKeyScale;
    public const int keyCount = 8;
    [Range(0,keyCount - 1)]
    public int note = 0;
    protected ColorRange colorRange = new ColorRange(keyCount - 1, Color.red);

    protected override void awake () {
        base.awake();
        whiteKeyScale = FindObjectOfType<WhiteKeysScale>();
	}

    protected override string getNoteName() {
        return whiteKeyScale.noteName(note);
    }

    protected override Color getColor() {
        return colorRange[note];
    }

    ////protected AudioSource _audioSource;
    //    protected WhiteKeysScale whiteKeyScale;
    //    public const int keyCount = 8;
    //    [Range(0,keyCount - 1)]
    //    public int note = 0;
    //    protected ColorRange colorRange = new ColorRange(keyCount - 1, Color.red);
    //    protected Highlighter highlighter;

    //    void Awake () {
    //        whiteKeyScale = FindObjectOfType<WhiteKeysScale>();
    //        highlighter = GetComponent<Highlighter>();
    //	}

    //    public void proxyCollisionEnter(Collision collision) {
    //        AudioEntity ae = AudioManager.Instance.getAudioEntityFor(this, whiteKeyScale.noteName(note));
    //        if (!ae.getAudioSource().isPlaying || ae.getAudioSource().time > .9f) {
    //            AudioManager.Instance.play(this, whiteKeyScale.noteName(note));
    //            highlighter.highlightForSeconds(.9f, colorRange[note]);
    //        }
    //    }

    //    public void proxyCollisionExit(Collision collision) {
    //    }

    //    public void proxyCollisionStay(Collision collision) {
    //    }
}
