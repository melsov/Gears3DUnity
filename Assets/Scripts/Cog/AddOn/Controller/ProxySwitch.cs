using UnityEngine;
using System.Collections;

public class ProxySwitch : Switch , IProxyAddOn {

    public void toggle() {
        toggleOn();
    }
}

public interface IProxyAddOn
{
}
