using UnityEngine;
using System.Collections;

public class LightSwitchIndicator : OnOffIndicator {

    public Vector3 spanEulerAngles = new Vector3(0f, 90f, 0f);

    protected Vector3 direction {
        get {
            return spanEulerAngles * (_state == SwitchState.ON ? .5f : -.5f);
        }
    }
    // LIGHT SWITCH INDICATOR NEEDS A RE-WORK: INDICATORS DON'T ACTUALLY KNOW ABOUT ADD-ONS PER SE.
    void Awake() {
        throw new System.Exception("dont use light switch indicators please");
    }

    //protected Vector3 indicatorPosition {
    //    get {
    //        VectorXZ dir = new VectorXZ(Mathf.Cos(Mathf.Deg2Rad * spanEulerAngles.y * .5f), Mathf.Sin(Mathf.Deg2Rad * spanEulerAngles.y * .5f)).normalized;
    //        VectorXZ dif = new VectorXZ(-addOn.transform.position + transform.position); // want local position actually?
    //        dir.x *= ((dif.x > 0) == (dir.x > 0)) ? 1f : -1f;
    //        if (_state != SwitchState.ON) {
    //            dir.z *= -1;
    //        }
    //        return (dir * dif.magnitude).vector3(transform.localPosition.y);
    //    }
    //}

    //protected override void updateIndicator() {
    //    base.updateIndicator();
    //    transform.rotation = Quaternion.Euler(direction);
    //    transform.localPosition = indicatorPosition;
        
    //    //Quaternion q = Quaternion.FromToRotation(transform.rotation * transform.localPosition, direction);
    //    //transform.RotateAround(addOn.transform.position, EnvironmentSettings.towardsCameraDirection, q.eulerAngles.y);
    //}

}
