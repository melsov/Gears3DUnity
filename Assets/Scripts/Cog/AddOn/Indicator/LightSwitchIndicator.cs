using UnityEngine;
using System.Collections;

public class LightSwitchIndicator : OnOffIndicator {

    public Vector3 spanEulerAngles = new Vector3(0f, 90f, 0f);

    protected Vector3 direction {
        get {
            return spanEulerAngles * (_state ? .5f : -.5f);
        }
    }

    protected Vector3 indicatorPosition {
        get {
            VectorXZ dir = new VectorXZ(Mathf.Cos(Mathf.Deg2Rad * spanEulerAngles.y * .5f), Mathf.Sin(Mathf.Deg2Rad * spanEulerAngles.y * .5f)).normalized;
            VectorXZ dif = new VectorXZ(-addOn.transform.position + transform.position); // want local position actually?
            dir.x *= ((dif.x > 0) == (dir.x > 0)) ? 1f : -1f;
            if (!_state) {
                dir.z *= -1;
            }
            return (dir * dif.magnitude).vector3(transform.localPosition.y);
        }
    }

    protected override void updateIndicator() {
        base.updateIndicator();
        transform.rotation = Quaternion.Euler(direction);
        transform.localPosition = indicatorPosition;
        
        //Quaternion q = Quaternion.FromToRotation(transform.rotation * transform.localPosition, direction);
        //transform.RotateAround(addOn.transform.position, EnvironmentSettings.towardsCameraDirection, q.eulerAngles.y);
    }

}
