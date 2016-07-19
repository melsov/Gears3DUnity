using UnityEngine;
using System.Collections;

public class ProxySwitch : Switch , IProxyAddOn {
    public void doToggle() {
        base.toggle();
    }
}

public interface IProxyAddOn
{
}
