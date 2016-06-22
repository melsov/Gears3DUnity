using UnityEngine;
using System.Collections;

public class AddOnConnector : MonoBehaviour {

    protected Drivable drivable;
    public void Awake() {
        drivable = GetComponent<Drivable>();
    }

    private AddOn findAddOn(Collider other) {
        AddOn addOn = other.GetComponentInParent<AddOn>();
        if (addOn == null) {
            IControllerAddOnProvider icaop = other.GetComponentInParent<IControllerAddOnProvider>();
            if (icaop != null) {
                addOn = icaop.getControllerAddOn();
            }
        }
        return addOn;
    }

    public Drivable.DrivableConnection getDrivableConnection(Collider other) {
        AddOnDrivableConnection aodc = new AddOnDrivableConnection(drivable);
        AddOn addOn = findAddOn(other);
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
