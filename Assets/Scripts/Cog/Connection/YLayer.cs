﻿using UnityEngine;
using System.Collections.Generic;

/*
codify y positions for all cog types
*/
public class YLayer {
    public const float LayerHeight = .75f;
    private static float wall = -.5f;
    private static float cogLayer = wall + LayerHeight;
    private static float cogLayerRope = -3f;
    private static float stickerLayer = 4.5f;
    private static Dictionary<System.Type, float> lookup = new Dictionary<System.Type, float>() {
        { typeof(Motor) , wall },
        { typeof(HandCrank) , wall },
        { typeof(Rope), cogLayerRope },
        { typeof(Sticker), stickerLayer },
    };

    public static float Layer(System.Type type) {
        if (lookup.ContainsKey(type)) { return lookup[type]; }
        return cogLayer;
    }
}

