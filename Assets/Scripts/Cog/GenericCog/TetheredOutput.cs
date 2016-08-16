using UnityEngine;
using System.Collections;
using System;

//Connects, passes logic from input. 
//Tethered pair now just wakes up. Setups up its (children) input and output, unparents them from itself
//And permanently goes to sleep?
//Input and output are both logic gates (Not not gates)

namespace Tethered
{

    public class TetheredOutput : SwitchExtension
    {
        private TetheredInput _input;
        public TetheredInput input {
            private get { return _input; }
            set {
                if (!_input && value) {
                    Debug.LogError("----||||---- value in TethO: " + value.name);
                    _input = value;
                }
            }
        }

        protected override int maxClients {
            get {
                return 1;
            }
        }

        protected override int maxProducers {
            get {
                return 1;
            }
        }

        protected ISwitchStateProvider producer {
            get {
                return input;
            }
        }

        protected override SwitchState calculateState(ISwitchStateProvider ignore) {
            print("teth output calc state. scalar null? " + (setScalar == null));
            if (producer == null || producer == ignore) {
                print("TethOUT ret OFF prod null? " + (producer == null) + " prod == ignore? " + (producer == ignore));
                return SwitchState.OFF;
            }
            return producer.currentState();
        }
    }
}
