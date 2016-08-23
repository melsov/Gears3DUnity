using UnityEngine;
using System.Collections.Generic;
using System;

/*
codify y positions for all cog types
*/
public class YLayer {
    public const float LayerHeight = 1f;
    private static readonly float wall = -.5f;
    private static readonly float cog = wall + LayerHeight;
    public static readonly float logicHousing = cog;
    public static readonly float gearMeshBase = cog + LayerHeight;
    private static readonly float rope = -3f;
    public static readonly float dispensable = 7f;
    private static readonly float sticker = 7.5f;
    public static readonly float camera = 20f;

    private static Dictionary<Type, float> lookup = new Dictionary<Type, float>() {
        { typeof(Motor) , wall },
        { typeof(HandCrank) , wall },
        { typeof(Rope), rope },
        { typeof(Sticker), sticker },
        { typeof(Dispensable), dispensable },
        { typeof(ExchangablePlaceable), dispensable },
    };

    public static float Layer(Type type) {
        if (lookup.ContainsKey(type)) { return lookup[type]; }
        return cog;
    }

    public static void moveToLayer(Transform t, Type type) {
        t.position = TransformUtil.SetY(t.position, lookup.ContainsKey(type) ? lookup[type] : cog);
    }

    public static void moveToDispensableLayer(Transform t) {
        moveToLayer(t, typeof(Dispensable));
    }
}

public class Floatster
{
    public readonly float Value;
    public Floatster(float f) { Value = f; }
    public static implicit operator float(Floatster f) { return f.Value; }
    public static implicit operator Floatster (float f) { return new Floatster(f); }
}

public class ComponentLayerBoss
{
    public class LayerTag
    {
        public struct LookUpLayer
        {
            public readonly string tag_;
            public readonly float layer_;

            public LookUpLayer(string tag, float layer) {
                tag_ = tag; layer_ = layer;
            }
        }

        public static string LLogic = "Logic";
        public static string LDispensable = "Dispensable";

        public static LookUpLayer[] tags = new LookUpLayer[] {
            new LookUpLayer(LLogic, YLayer.logicHousing),
            new LookUpLayer(LDispensable, YLayer.dispensable),
        };

        public readonly Transform logic;
        public readonly Transform dispensable;

        private static int Count { get { return tags.Length; } }

        protected LayerTag(params Transform[] ts) {
            UnityEngine.Assertions.Assert.IsTrue(ts.Length == tags.Length, "hmmm");
            logic = ts[0];
            dispensable = ts[1];
        }

        public static LayerTag FromCog(Cog cog) {
            Transform[] ts = new Transform[tags.Length];
            for(int i = 0; i < ts.Length; ++i) {
                ts[i] = TransformUtil.FindTagWithin(cog.transform, tags[i].tag_);
            }
            return new LayerTag(ts);
        }
    }

    private readonly WeakReference _cog;
    protected Cog cog { get { return (Cog)_cog.Target; } }

    public ComponentLayerBoss(Cog cog) {
        _cog = new WeakReference(cog);
    }

    public void adjust() {
        foreach (LayerTag.LookUpLayer lul in LayerTag.tags) {
            foreach (Transform t in TransformUtil.FindAllEldestGenerationWithTag(cog.transform, lul.tag_)) {
                adjust(t, lul.layer_);
            }
        }
    }

    private static void adjust(Transform t, float layer) {
        if (!t || t.position.y == layer) { return; }
        
        if (t.GetComponent<Rigidbody>()) {
            t.GetComponent<Rigidbody>().MovePosition(TransformUtil.SetY(t.GetComponent<Rigidbody>().position, layer));
        } else {
            t.transform.position = TransformUtil.SetY(t.position, layer);
        }
    }
    
}

