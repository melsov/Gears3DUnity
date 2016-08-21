using UnityEngine;
using System.Collections.Generic;

/*
codify y positions for all cog types
*/
public class YLayer {
    public const float LayerHeight = 1f;
    private static readonly float wall = -.5f;
    private static readonly float cog = wall + LayerHeight;
    private static readonly float rope = -3f;
    public static readonly float dispenseable = 7f;
    private static readonly float sticker = 7.5f;
    public static readonly float camera = 20f;

    private static Dictionary<System.Type, float> lookup = new Dictionary<System.Type, float>() {
        { typeof(Motor) , wall },
        { typeof(HandCrank) , wall },
        { typeof(Rope), rope },
        { typeof(Sticker), sticker },
        { typeof(Dispensable), dispenseable },
        { typeof(ExchangablePlaceable), dispenseable },
    };

    public static float Layer(System.Type type) {
        if (lookup.ContainsKey(type)) { return lookup[type]; }
        return cog;
    }

    public static void moveToLayer(Transform t, System.Type type) {
        t.position = TransformUtil.SetY(t.position, lookup.ContainsKey(type) ? lookup[type] : cog);
    }

    public static void moveToDispensableLayer(Transform t) {
        moveToLayer(t, typeof(Dispensable));
    }
}

