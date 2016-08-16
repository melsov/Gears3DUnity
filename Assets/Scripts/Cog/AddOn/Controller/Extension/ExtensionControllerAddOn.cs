using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//TODO: electromagnets reverse their magnets by graphically reversing the current
//CONSIDER: magnets don't need 'reverse' properties; they can simply rotate 180 around the y axis or move north transform locally to south
//BUG: Two motors connected to a NotSwitch, the eastward motor re-aligned strangely when the switch was dragged
//TODO: LogicPiston (related: make PistonMech; a component mechanism that is a piston)

//CONSIDER: to draw bezier '' curves, just use Unity's AnimationCurves. input = normalized relative x pos. out: scaled z pos (and original scale x pos)

public abstract class ExtensionControllerAddOn : ControllerAddOn {

    protected abstract int maxClients {
        get;
    }
    protected abstract int maxProducers {
        get;
    }

    protected int clientCount {
        get {
            int result = 0;
            foreach(Cog c in clients()) { ++result; }
            return result;
        }
    }

    protected int producerCount {
        get {
            int result = 0;
            foreach(Cog c in producerCAOs()) { ++result; }
            return result;
        }
    }

    protected IEnumerable<Cog> clients() {
        foreach(Cog cog in contractPortfolio.clients()) {
            yield return cog;
        }
    }

    protected IEnumerable<ControllerAddOn> producerCAOs() {
        foreach(Cog cog in contractPortfolio.producers()) {
            if (cog is ControllerAddOn) { yield return (ControllerAddOn)cog; }
        }
    }
    
    #region contract

    protected override ContractNegotiator getContractNegotiator() {
        return new ExtensionAddOnContractNegotiator(this);
    }

    public class ExtensionAddOnContractNegotiator : CAOContractNegotiator
    {
        public ExtensionAddOnContractNegotiator(Cog cog_) : base(cog_) { }

        protected override List<ContractSpecification> orderedContractPreferencesAsOfferer(Cog cogForTypeWorkaround) {
            List<ContractSpecification> prefs = new List<ContractSpecification>();
            prefs.Add(new ContractSpecification(CogContractType.CONTROLLER_ADDON_DRIVABLE, RoleType.CLIENT));
            prefs.Add(new ContractSpecification(CogContractType.CONTROLLER_ADDON_DRIVABLE, RoleType.PRODUCER));
            return prefs;
        }
    }

    protected override ViableContractLookup getViableContractLookup() {
        return new ViableExtensionControllerAddOnContractLookup(this);
    }

    public class ViableExtensionControllerAddOnContractLookup : ViableCAOContractLookup
    {
        private WeakReference _extensionCAO;
        private ExtensionControllerAddOn extensionCAO { get { return (ExtensionControllerAddOn)_extensionCAO.Target; } }
        public ViableExtensionControllerAddOnContractLookup(Cog cog_) : base(cog_) {
            _extensionCAO = new WeakReference(cog_);
        }

        protected override void setupLookups() {
            asProducerLookup.Add(CogContractType.CONTROLLER_ADDON_DRIVABLE, delegate(Cog client) { return extensionCAO.clientCount < extensionCAO.maxClients; });
            asClientLookup.Add(CogContractType.CONTROLLER_ADDON_DRIVABLE, delegate(Cog producer) {
                return extensionCAO.canBeClientOf(producer) && extensionCAO.producerCount < extensionCAO.maxProducers;
            });
        }
    }

    public override ClientActions clientActionsFor(Cog producer, ContractSpecification specification) {
        if(specification.contractType == CogContractType.CONTROLLER_ADDON_DRIVABLE) {
            ClientActions cas = new ClientActions();
            cas.receive = delegate (Cog _producer) {
                ControllerAddOn cao = (ControllerAddOn)findAddOn(_producer);
                cao.addSetScalar(handleProducerSetScalar);
                onAddProducer(cao);
                Debug.LogError("receiving addOn contr. " + name + " prod is: " + _producer.name + " prod set scalar null? " + cao.debugSetScalarIsNull());
            };
            cas.beAbsolvedOf = delegate (Cog _producer) {
                Bug.debugIfIs<Tethered.TetheredOutput>(this, name + "absolving contract");
                ((ControllerAddOn)findAddOn(_producer)).removeSetScalar(handleProducerSetScalar);
                onRemoveProducer((ControllerAddOn)findAddOn(_producer));
            };
            return cas;
        }
        return ClientActions.getDoNothingActions();
    }

    #endregion

    protected abstract bool canBeClientOf(Cog cog);

    protected virtual void onAddProducer(ControllerAddOn cao) {
        updateState();
    }

    protected virtual void onRemoveProducer(ControllerAddOn cao) {
        updateState(cao);
    }

    protected void handleProducerSetScalar(float scalar) {
        updateState();
    }

    protected void updateState() {
        updateState(null);
    }

    protected abstract void updateState(ControllerAddOn ignore);

    
}
