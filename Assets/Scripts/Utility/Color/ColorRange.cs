using UnityEngine;
using System.Collections;

public class ColorRange
{
    protected int increments;
    protected HSBColor start;

    public ColorRange(int _increments, Color _startColor) {
        increments = _increments;
        start = new HSBColor(_startColor);
    }

    public Color this[int index] {
        get {
            float hue = start.h + index / ((float)increments); // Mathf.Clamp01(index / increments); // Angles.FloatModSigned(start.h + Mathf.Clamp01(index / increments), 1f);
            return new HSBColor(hue, start.s, start.b).ToColor();
        }
    }

}
