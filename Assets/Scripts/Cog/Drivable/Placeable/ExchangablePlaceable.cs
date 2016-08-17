using UnityEngine;
using System.Collections;

public class ExchangablePlaceable : Placeable {

    private Exchangable _exchangable;
    protected Exchangable exchangable {
        get {
            if(!_exchangable) {
                _exchangable = GetComponentInChildren<Exchangable>();
            }
            return _exchangable;
        }
    }

    public override void OnEnable() {
        base.OnEnable();
        exchangable.register(handleOnExchanged);
    }

    public override void OnDisable() {
        base.OnDisable();
        exchangable.unregister(handleOnExchanged);
    }

    protected virtual void handleOnExchanged(Exchangable ex) {

    }

    public override void normalDragStart(VectorXZ cursorPos) {
        exchangable.closedDown = true;
        print("ex closed down: " + exchangable.closedDown);
        base.normalDragStart(cursorPos);
    }

    public override void normalDrag(VectorXZ cursorPos) {
        base.normalDrag(cursorPos);
    }

    public override void normalDragEnd(VectorXZ cursorPos) {
        base.normalDragEnd(cursorPos);
        print("END ex closed down: " + exchangable.closedDown);
        exchangable.closedDown = false;
        exchangable.detectExchangers();
    }
}
