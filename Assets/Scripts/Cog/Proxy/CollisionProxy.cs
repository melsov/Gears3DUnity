using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class CollisionProxy : MonoBehaviour {

    private ICollisionProxyClient client;

	void Awake () {
        Assert.IsTrue(GetComponent<Collider>() != null);
        Assert.IsTrue(GetComponent<Collider>().isTrigger == false);
        client = GetComponentInParent<ICollisionProxyClient>(); 
        Assert.IsTrue(client != null);
    }
	
    void OnCollisionEnter(Collision collision) {
        client.proxyCollisionEnter(collision);
    }
    void OnCollisionStay(Collision collision) {
        client.proxyCollisionStay(collision);
    }
    void OnCollisionExit(Collision collision) {
        client.proxyCollisionExit(collision);
    }
}

public interface ICollisionProxyClient
{
    void proxyCollisionEnter(Collision collision);
    void proxyCollisionStay(Collision collision);
    void proxyCollisionExit(Collision collision);
}
