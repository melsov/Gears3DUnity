using UnityEngine;
using System.Collections;
using Tethered;

public class TetheredPair : MonoBehaviour {
     
	public void Start() {
        TetheredInput input = TransformUtil.GetComponentOnlyInChildren<TetheredInput>(transform);
        TetheredOutput output = TransformUtil.GetComponentOnlyInChildren<TetheredOutput>(transform);
        if (!input || !output) { return; }
        input.output = output;

        input.transform.SetParent(null);
        output.transform.SetParent(null);
        gameObject.SetActive(false);
    }
}
