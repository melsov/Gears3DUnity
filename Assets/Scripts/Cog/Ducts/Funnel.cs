using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Funnel : Duct , IGameSerializable {

    public float strength = 20f;
    
    void OnTriggerEnter(Collider other) {
        pullToCenter(other);
    }

    void OnTriggerStay(Collider other) {
        pullToCenter(other);
    }

    //TODO: make funnel actually pull to center
    private void pullToCenter(Collider other) {
        Vector3 towards = transform.position - other.transform.position;
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;
        float dot = Vector3.Dot(towards, transform.rotation * EnvironmentSettings.gravityDirection);
        if (dot > 0f) { 
            rb.velocity = Vector3.Lerp(towards.normalized, rb.velocity.normalized, .5f) * strength * towards.magnitude;
        } else {
            rb.velocity = Vector3.Lerp(transform.rotation * EnvironmentSettings.gravityDirection, rb.velocity.normalized, .5f) * rb.velocity.magnitude * .95f;
        }
    }

    #region serialize
    [System.Serializable]
    class SerializeStorage
    {
        public float strength;
    }
    public void Serialize(ref List<byte[]> data) {
        SerializeStorage ss = new SerializeStorage();
        ss.strength = strength;

        SaveManager.Instance.SerializeIntoArray(ss, ref data);
    }

    public void Deserialize(ref List<byte[]> data) {
        SerializeStorage stor;
        if((stor = SaveManager.Instance.DeserializeFromArray<SerializeStorage>(ref data)) != null) {
            strength = stor.strength;
        }
    }
    #endregion
}
