using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;

/*
Base class for all mechanisms
*/
[System.Serializable]
public abstract class Cog : MonoBehaviour {

    private ContractManager contractManager;

    //CONSIDER: THERE COULD BE A STATIC PROVIDER FOR VCLS based on type: VCLs are not instance specific are they?
    protected ViableContractLookup _viableContractLookup;
    protected ViableContractLookup viableContractLookup {
        get {
            if (_viableContractLookup == null) {
                _viableContractLookup = getViableContractLookup();
            }
            return _viableContractLookup;
        }
    }

    protected virtual ViableContractLookup getViableContractLookup() {
        return new ViableContractLookup(this);
    }

    protected ColliderSet colliderSet;
    protected NeighborColliderLookup neighborColliderLookup;

    public void Awake() {
        awake();
    }

    protected virtual void awake() {
        if (!colliderSet) {
            colliderSet = gameObject.AddComponent<ColliderSet>();
        }
        contractManager = new ContractManager(this, getContractNegotiator());
        //if (!neighborColliderLookup) {
        //    neighborColliderLookup = gameObject.AddComponent<NeighborColliderLookup>();
        //}
    }

    public void disableColliders() {
        colliderSet.disableForNextFixedFrame();
    }

    void OnDestroy() {
        vOnDestroy();
    }

    protected virtual void vOnDestroy() {
        //AudioManager.Instance.remove(this);
    }

    public static Cog FindCog(Transform trans) {
        return FindInCog<Cog>(trans);
    }
    public static T FindInCog<T>(Transform trans) {
        Cog cog = trans.GetComponentInParent<Cog>();
        if (cog == null) { return default(T); }
        return cog.GetComponentInChildren<T>();
    }

    protected void highlight(Transform other) {
        Highlighter h = FindInCog<Highlighter>(other);
        if (h == null) { return; }
        h.highlight();
    }

    protected void unhighlight(Transform other) {
        Highlighter h = FindInCog<Highlighter>(other);
        if (h == null) { return; }
        h.unhighlight();
    }

    public void positionRelativeToAddOn(AddOn addOn) {
        Vector3 pos = transform.position;
        Transform target = addOn.transform;
        if (addOn is IProxyAddOn) {
            target = FindInCog<Drivable>(FindCog(addOn.transform).transform.parent).transform;
        }
        pos.x = target.position.x;
        Collider other = FindInCog<Collider>(target);
        Vector3 extents = GetComponentInParent<Collider>().bounds.extents;
        pos.z = other != null ? (other.bounds.min - extents).z : pos.z;
        transform.position = pos;
    }

    #region contract

    public class ContractManager
    {
        private WeakReference _cog;
        public Cog cog { get { return (Cog)_cog.Target;  } }

        private ContractNegotiator negotiator;
        private List<CogContract> contracts = new List<CogContract>();

        public ContractManager(Cog cog_, ContractNegotiator negotiator_) {
            _cog = new WeakReference(cog_);
            negotiator = negotiator_;
        }

        public virtual bool hasContractWith(ContractManager other) {
            foreach(CogContract cc in contracts) {
                if (cc.producer == other || cc.client == other) { return true; }
            }
            return false;
        }

        private bool isProducerFor(CogContract cc) {
            return cc.producer == this;
        }

        public virtual ContractSpecification negotiate(ContractManager other) {
            print("negotiate");
            if (other == null) {
                print("other ccm null in other found by " + cog.name);
            }
            foreach(ContractSpecification specification in negotiator.contractPreferencesAsOffererWith(other.cog)) {
                if (other.amenable(cog, specification)) {
                    print("amenable");
                    return specification;
                }
            }
            return ContractSpecification.NonExistant();
        }

        protected virtual bool amenable(Cog other, ContractSpecification specification) {
            return negotiator.amenable(other, specification);
        }

        
        /*
        * Caller makes contract with itself as owner
        *  */
        public virtual void setup(ContractManager client, ContractSpecification specification) {
            if (!specification.offererIsProducer) {
                specification.offererIsProducer = true;
                client.setup(this, specification);
                return;
            }
            CogContract cc = new CogContract(this, 
                client, 
                specification.contractType, 
                cog.producerActionsFor(client.cog, specification), 
                client.cog.clientActionsFor(client.cog, specification));
            client.accept(cc);
            cc.producerActions.initiate(cc.client.cog);
            contracts.Add(cc);
        }

        protected virtual void accept(CogContract cc) {
            cc.clientActions.receive(cc.producer.cog);
            contracts.Add(cc);
        }

        public virtual void dissolve(CogContract cc) {
            cc.producer.dissolveAsProducer(cc);
        }

        private void dissolveAsProducer(CogContract cc) {
            cc.client.forget(cc);
            cc.producerActions.dissolve(cc.client.cog);
            contracts.Remove(cc);
        }

        protected virtual void forget(CogContract cc) {
            cc.clientActions.beAbsolvedOf(cc.producer.cog);
            contracts.Remove(cc);
        }

        internal void getOutOfAll() {
            while(contracts.Count > 0) {
                dissolve(contracts[0]);
            }
        }
    }

    public class ContractNegotiator
    {
        private ViableContractLookup lookup {
            get { return cog.viableContractLookup; }
        }
        private WeakReference _cog;
        protected Cog cog {
            get { return (Cog)_cog.Target; }
        }

        public ContractNegotiator(Cog cog_) { 
            _cog = new WeakReference(cog_);
        }

        public virtual bool amenable(Cog other, ContractSpecification specification) {
            return amenableFor(lookup.availabilty(other, specification.contractType), specification.offererIsProducer);
        }

        public virtual bool couldBeEither(Cog other, ContractSpecification specifiaction) {
            return lookup.availabilty(other, specifiaction.contractType) == RoleAvailablity.CAN_BE_PRODUCER_OR_CLIENT;
        }

        private static bool amenableFor(RoleAvailablity ra, bool otherWantsProducer) {
            if (ra == RoleAvailablity.UNAVAILABLE) { return false; }
            if (otherWantsProducer) { return ra != RoleAvailablity.CAN_ONLY_BE_PRODUCER; }
            return ra != RoleAvailablity.CAN_ONLY_BE_CLIENT;
        }

        /*
         * from a list of Contract Specs 'top choices' 
         * provided on a per one or more Cog type basis
         * but not on an instance specific basis
         * use Contract Viability lookup to cull the non viable ones, 
         * on an instance specific basis
         * */
        protected virtual List<ContractSpecification> orderedContractPreferencesAsOfferer(Cog cogForTypeWorkaround) {
            print("cog empty ordered list");
            return new List<ContractSpecification>();
        }

        public virtual List<ContractSpecification> contractPreferencesAsOffererWith(Cog other) {
            List<ContractSpecification> result = new List<ContractSpecification>();
            foreach(ContractSpecification specification in orderedContractPreferencesAsOfferer(other)) {
                if (canOffer (other, specification)) {
                    print("I, " + cog.name + ", can offer " + other.name );
                    result.Add(specification);
                }
            }
            return result;
        }

        private bool canOffer(Cog cog, ContractSpecification specification) {
            if (specification.offererIsProducer) {
                return lookup.availableForProducerRole(cog, specification.contractType);
            } else {
                return lookup.availableForClientRole(cog, specification.contractType);
            }
        }
    }

    public abstract ProducerActions producerActionsFor(Cog client, ContractSpecification specification);
    public abstract ClientActions clientActionsFor(Cog producer, ContractSpecification specification);
    
    protected virtual ContractNegotiator getContractNegotiator() {
        return new ContractNegotiator(this);
    }
    //public class ContractActions
    //{
    //    public delegate void ContractAction(Cog other);
    //    public ContractAction initiate;
    //    public ContractAction receive;
    //    public ContractAction fulfill;
    //    public ContractAction dissolve;
    //    public ContractAction beAbsolvedOf;
    //}
    public class ProducerActions
    {
        public delegate void ProducerAction(Cog other);
        public ProducerAction initiate;
        public ProducerAction fulfill;
        public ProducerAction dissolve;

        public static ProducerActions getDoNothingActions() {
            ProducerAction nothingAction = delegate (Cog cog) { print("producer nothing action for client " + cog.name); };
            ProducerActions pa = new ProducerActions();
            pa.initiate = nothingAction;
            pa.fulfill = nothingAction;
            pa.dissolve = nothingAction;
            return pa;
        }
    }

    public class ClientActions {
        public delegate void ClientAction(Cog other);
        public ClientAction receive;
        public ClientAction beAbsolvedOf;

        public static ClientActions getDoNothingActions() {
            ClientAction nothingAction = delegate (Cog cog) { print("client nothing action for producer: " + cog.name); };
            ClientActions ca = new ClientActions();
            ca.receive = nothingAction;
            ca.beAbsolvedOf = nothingAction;
            return ca;
        }

    }

    protected virtual bool vConnectTo(Collider other) {
        Cog cog = FindCog(other.transform);
        if (cog == null) { return false; }
        ContractSpecification spec = contractManager.negotiate(cog.contractManager);
        if(spec.contractType != CogContractType.NONEXISTENT) {
            contractManager.setup(cog.contractManager, spec);
            return true;
        }
        return false;
    }

    protected virtual void vDisconnect() {
        contractManager.getOutOfAll();
    }

    /*
    * 
    * */
    public class ViableContractLookup
    {
        private WeakReference _cog;
        public Cog cog {
            get { return (Cog)_cog.Target; }
        }

        protected delegate bool Amenable(Cog other);
        protected Dictionary<CogContractType, Amenable> asProducerLookup = new Dictionary<CogContractType, Amenable>();
        protected Dictionary<CogContractType, Amenable> asClientLookup = new Dictionary<CogContractType, Amenable>();

        protected Amenable nope = (delegate (Cog other) { return false; });
        protected Amenable yup = (delegate (Cog other) { return true; });


        public ViableContractLookup(Cog cog_) {
            _cog = new WeakReference(cog_);
            setupLookups();
        }

        protected virtual void setupLookups() {
            //refuse all by default
            foreach(CogContractType cct in Enum.GetValues(typeof(CogContractType))) {
                asProducerLookup.Add(cct, nope);
                asClientLookup.Add(cct, nope);
            }
        }

        public bool availableForProducerRole(Cog other, CogContractType cct) {
            if (asProducerLookup.ContainsKey(cct)) {
                return asProducerLookup[cct](other);
            }
            return false;
        }

        public bool availableForClientRole(Cog other, CogContractType cct) {
            if (asClientLookup.ContainsKey(cct)) {
                return asClientLookup[cct](other);
            }
            return false;
        }

        public RoleAvailablity availabilty(Cog other, CogContractType cct) {
            bool canBeProducer = availableForProducerRole(other, cct);
            bool canBeClient = availableForClientRole(other, cct);
            if (canBeProducer && canBeClient) { return RoleAvailablity.CAN_BE_PRODUCER_OR_CLIENT; }
            if (canBeProducer) { return RoleAvailablity.CAN_ONLY_BE_PRODUCER; }
            if (canBeClient) { return RoleAvailablity.CAN_ONLY_BE_CLIENT; }
            return RoleAvailablity.UNAVAILABLE;
        }
    }

    #endregion

    public static implicit operator bool(Cog exists) {
        return exists != null;
    }

    public static AddOn findAddOn(Collider other) {
        AddOn addOn = other.GetComponentInParent<AddOn>();
        if (addOn == null) {
            IControllerAddOnProvider icaop = other.GetComponentInParent<IControllerAddOnProvider>();
            if (icaop != null) {
                addOn = icaop.getControllerAddOn();
            }
        }
        return addOn;
    }

}
