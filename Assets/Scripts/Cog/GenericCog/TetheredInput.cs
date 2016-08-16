using UnityEngine;
using System.Collections;

namespace Tethered
{
    public class TetheredInput : SwitchExtension
    {

        private TetheredOutput _output;
        public TetheredOutput output {
            private get { return _output; }
            set {
                if (!_output && value) {
                    _output = value;
                    _output.input = this;
                    forcePermanentEarmarkedContract(_output, Earmark.A, CogContractType.CONTROLLER_ADDON_DRIVABLE);
                }
            }
        }

        public override void addSetScalar(SetScalar setScalar) {
            base.addSetScalar(setScalar);
            Debug.LogError("teth input addSetScalar. null? " + (setScalar == null));
        }

        //internal void setSetScalar(SetScalar setScalar) {
        //    this.setScalar = setScalar;
        //}

        public override ContractPortfolio.ClientTree.Node node {
            get {
                if (_node == null) {
                    /* Node that doesn't propagate moves to its children */
                    _node = new ContractPortfolio.ClientTree.Node(contractPortfolio, false);
                }
                return _node;
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
                foreach (ISwitchStateProvider s in producerSSPs()) {
                    return s;
                }
                return null;
            }
        }

        protected override SwitchState calculateState(ISwitchStateProvider ignore) {
            print("input calc state ");
            if (producer == null || producer == ignore) { return SwitchState.OFF; }
            return producer.currentState();
        }
    }
}