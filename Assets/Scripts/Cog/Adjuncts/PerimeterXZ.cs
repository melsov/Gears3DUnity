using UnityEngine;
using System.Collections.Generic;

public class PerimeterXZ {

    protected static Dictionary<int, VectorXZ[]> lookup = new Dictionary<int, VectorXZ[]>();

    public static VectorXZ[] directions(int resolution) {
        if (!lookup.ContainsKey(resolution)) {
            lookup.Add(resolution, new VectorXZ[resolution]);
            for (int i = 0; i < lookup[resolution].Length; ++i) {
                lookup[resolution][i] = new VectorXZ(Quaternion.Euler(new Vector3(0f, i * 360f / ((float)resolution), 0f)) * Vector3.right);
            }
        }
        return lookup[resolution];
    }

    public static VectorXZ[] directions12() { return directions(12); }

}
