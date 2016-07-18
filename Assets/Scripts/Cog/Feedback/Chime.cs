using System;
using UnityEngine;

public class Chime : Instrument {
    protected WhiteKeysScale whiteKeyScale;
    public const int keyCount = 8;
    [Range(0,keyCount - 1)]
    public int note = 0;
    protected ColorRange colorRange = new ColorRange(keyCount - 1, Color.red);
    protected static string WholeNoteLetters = "cdefgabc";

    protected override void awake () {
        base.awake();
        whiteKeyScale = FindObjectOfType<WhiteKeysScale>();
        addNoteText();
	}

    private void addNoteText() {
        TextMesh textMesh = GetComponentInChildren<TextMesh>();
        if (!textMesh) {
            textMesh = Instantiate<TextMesh>(Resources.Load<TextMesh>("Prefabs/Text/NoteText"));
            Transform chimeMesh =  GetComponentInChildren<CollisionProxy>().transform;
            textMesh.transform.parent = chimeMesh;
            textMesh.transform.localScale = MathVector3.div(Vector3.one, chimeMesh.localScale);
            textMesh.transform.position = new Vector3(.6f, 0f, 0f);
        }
        if (textMesh) {
            textMesh.text = string.Format("{0}", WholeNoteLetters[note % keyCount]);
            textMesh.color = getColor();
        } 
    }

    protected override string getNoteName() {
        return whiteKeyScale.noteName(note);
    }

    protected override Color getColor() {
        return colorRange[note];
    }

    protected override ContractSiteBoss getConnectionSiteBoss() {
        throw new NotImplementedException();
    }

    public override ConnectionSiteAgreement.ConnektAction connektActionAsTravellerFor(ContractSpecification specification) {
        throw new NotImplementedException();
    }
}
