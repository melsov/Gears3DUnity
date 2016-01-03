using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

/* 
Relays trigger messages to parent client
*/
public class TriggerProxy : MonoBehaviour {

    protected ITriggerProxyClient client;

	void Awake () {
        Assert.IsTrue(GetComponent<Collider>() != null);
        GetComponent<Collider>().isTrigger = true;
        client = GetComponentInParent<ITriggerProxyClient>(); // TransformUtil.FindTypeInParentRecursive<ITriggerProxyClient>(transform, 3);
        Assert.IsTrue(client != null);
	}

    void OnTriggerEnter(Collider other) {
        client.proxyTriggerEnter(other);
    }
    void OnTriggerStay(Collider other) {
        client.proxyTriggerStay(other);
    }
    void OnTriggerExit(Collider other) {
        client.proxyTriggerExit(other);
    }

}

public interface ITriggerProxyClient
{
    void proxyTriggerEnter(Collider other);
    void proxyTriggerStay(Collider other);
    void proxyTriggerExit(Collider other);
}
