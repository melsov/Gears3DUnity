using UnityEngine;
using System.Collections;
using System;

public class OnOffFlag : MonoBehaviour , IOnOffIndicatorProxy {

    protected Arc arc;
    [SerializeField]
    protected Transform flag;
    public int transitionSegments = 15;
    public AnimationCurve curve;
    public string soundName = AudioLibrary.TwangSoundName;

    public void acceptState(SwitchState state) {
        StartCoroutine(activate(state == SwitchState.ON));
    }

    protected IEnumerator activate(bool _on) {
        if (_on) {
            AudioManager.Instance.play(GetComponentInParent<Cog>(), soundName);
        }
        for(int i = 0; i < transitionSegments; ++i) {
            float gradient = ((float)(i))/((float)transitionSegments);
            gradient = _on ? gradient : 1f - gradient;
            flag.rotation = arc.between(curve.Evaluate(gradient));
            yield return new WaitForFixedUpdate();
        }
        flag.rotation = arc.between(_on ? 1f : 0f);
    }
    
    public void Awake() {
        arc = GetComponentInChildren<Arc>();
    }
}
