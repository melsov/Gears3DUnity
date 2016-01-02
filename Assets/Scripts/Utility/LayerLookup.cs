using UnityEngine;
using System.Collections;

public class LayerLookup : MonoBehaviour {

    public static int IgnoreRaycast = LayerMask.NameToLayer("IgnoreRaycast");
    public static int Selectable = LayerMask.NameToLayer("Selectable");
    public static int DragOverride = LayerMask.NameToLayer("DragOverride");
    public static int CogComponent = LayerMask.NameToLayer("CogComponent");
}
