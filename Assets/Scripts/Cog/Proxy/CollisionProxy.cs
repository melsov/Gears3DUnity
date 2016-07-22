using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

//TODO: check if collision proxy is needed (and, for that matter, if 2 colliders are needed) (for proxy switch)
public class CollisionProxy : MonoBehaviour {

    private ICollisionProxyClient client;

	void Awake () {
        Assert.IsTrue(GetComponent<Collider>() != null);
        Assert.IsTrue(GetComponent<Collider>().isTrigger == false);
        client = GetComponentInParent<ICollisionProxyClient>(); 
        Assert.IsTrue(client != null, string.Format("No ICollisionProxyClient for {0} of cog {1}", name, Cog.FindCog(transform).name));
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
