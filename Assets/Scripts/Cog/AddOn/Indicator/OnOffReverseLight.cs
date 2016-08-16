using UnityEngine;
using System.Collections;
using System;

public class OnOffReverseLight : MonoBehaviour, IOnOffIndicatorProxy
{
    [SerializeField]
    protected Renderer _renderer;

    public void acceptState(SwitchState state) {
        throw new NotImplementedException();
    }
}
