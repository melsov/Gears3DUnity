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
        client = GetComponentInParent<ITriggerProxyClient>(); 
        Assert.IsTrue(client != null, " oh no null ITriggerProxyClient in " + name);
	}

    void OnTriggerEnter(Collider other) {
        relay(other, 0);
    }
    void OnTriggerStay(Collider other) {
        relay(other, 1);
    }
    void OnTriggerExit(Collider other) {
        relay(other, 2);
    }

    private void relay(Collider other, int state) {
        if (client == null) {
            if (Cog.FindCog(transform)) {
                print(Cog.FindCog(transform).name + " has a trigger proxy w/o client");
            }
            Bug.bugAndPause("null client in Trigger Proxy?? for: " + name);
            return;
        }
        switch (state) {
            case 0:
                client.proxyTriggerEnter(other);
                break;
            case 1:
                client.proxyTriggerStay(other);
                break;
            default:
                client.proxyTriggerExit(other);
                break;
        }
    }

}

public interface ITriggerProxyClient
{
    void proxyTriggerEnter(Collider other);
    void proxyTriggerStay(Collider other);
    void proxyTriggerExit(Collider other);
}
