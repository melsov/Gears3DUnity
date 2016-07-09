using UnityEngine;
using System.Collections;

public class ProxySwitch : Switch , IProxyAddOn {

    public void toggle() {
        base.toggle();
    }
}

public interface IProxyAddOn
{
}
