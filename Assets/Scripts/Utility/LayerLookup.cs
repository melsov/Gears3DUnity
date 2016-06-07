using UnityEngine;
using System.Collections;

public class LayerLookup : MonoBehaviour {

    public static int IgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
    public static int Selectable = LayerMask.NameToLayer("Selectable");
    public static int DragOverride = LayerMask.NameToLayer("DragOverride");
    public static int CogComponent = LayerMask.NameToLayer("CogComponent");
    public static int GearMesh = LayerMask.NameToLayer("GearMesh");

    public static int MaskAll = ~0;
}
