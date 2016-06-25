using UnityEngine;
using System.Collections;

public class AddOnConnector : MonoBehaviour {

    protected Drivable drivable;
    public void Awake() {
        drivable = GetComponent<Drivable>();
    }

    public Drivable.DrivableConnection getDrivableConnection(Collider other) {
        AddOnDrivableConnection aodc = new AddOnDrivableConnection(drivable);
        AddOn addOn = Cog.findAddOn(other);
        if (addOn) {
            print(" already client? " + addOn.hasClient);
        }
        if (addOn != null && !addOn.hasClient) {
            aodc.addOn = addOn;
            aodc.makeConnection = makeConnection;
        }
        return aodc;
    }

    protected bool makeConnection(Drivable.DrivableConnection dc) {
        AddOnDrivableConnection aodc = (AddOnDrivableConnection)dc;
        return aodc.addOn.connectToClient(drivable);
    }

    public class AddOnDrivableConnection : Drivable.DrivableConnection
    {
        public AddOn addOn;
        public AddOnDrivableConnection(Drivable _drivable) : base(_drivable) {
        }
    }
}
