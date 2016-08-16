using UnityEngine;
using System.Collections;

public class ColorRange
{
    protected readonly int increments;
    protected readonly HSBColor start;

    public ColorRange(int _increments, Color _startColor) {
        increments = _increments;
        start = new HSBColor(_startColor);
    }

    public Color this[int index] {
        get {
            float hue = start.h + index / ((float)increments); 
            return new HSBColor(hue, start.s, start.b).ToColor();
        }
    }

    public Color random() {
        return this[Random.Range(0, increments)];
    }

}
