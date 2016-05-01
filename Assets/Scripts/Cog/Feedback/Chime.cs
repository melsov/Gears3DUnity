using UnityEngine;

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

}
