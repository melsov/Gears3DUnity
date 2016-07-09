using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;

/*
Base class for all mechanisms
*/
[System.Serializable]
public abstract class Cog : MonoBehaviour, ICursorAgentUrClient {

    private ContractManager contractManager;
    protected ContractPortfolio contractPortfolio {
        get {
            return contractManager.contractPortfolio;
        }
    }
    private ConnectionSiteBoss _connectionSiteBoss;
    protected ConnectionSiteBoss connectionSiteBoss {
        get {
            if (_connectionSiteBoss == null) {
                _connectionSiteBoss = getConnectionSiteBoss();
            }
            return _connectionSiteBoss;
        }
    }
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

    private ContractPortfolio.ClientTree.Node _node;
    public ContractPortfolio.ClientTree.Node node {
        get {
            if (_node == null) {
                _node = new ContractPortfolio.ClientTree.Node(contractPortfolio);
            }
            return _node;
        }
    }

    private ContractPortfolio.ClientTree _clientTree;
    public ContractPortfolio.ClientTree clientTree {
        get {
            if (_clientTree == null) {
                _clientTree = new ContractPortfolio.ClientTree(node);
            }
            return _clientTree;
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

        if (GetComponent<Highlighter>() == null) {
            gameObject.AddComponent<Highlighter>();
        }
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
        public ContractPortfolio contractPortfolio; // = new ContractPortfolio(cog, null /* TODO: cog get connection site boss */);

        public ContractManager(Cog cog_, ContractNegotiator negotiator_) {
            _cog = new WeakReference(cog_);
            negotiator = negotiator_;
            contractPortfolio = new ContractPortfolio(cog, cog.connectionSiteBoss);
        }

        public virtual bool hasContractWith(ContractManager other) {
            foreach(CogContract cc in contractPortfolio) {
                if (cc.producer == other || cc.client == other) { return true; }
            }
            return false;
        }

        private bool isProducerFor(CogContract cc) {
            return cc.producer == this;
        }

        public bool hasAtleastOneContract {
            get { return contractPortfolio.hasAtleastOneContract; }
        }

        public virtual ContractSpecification negotiate(ContractManager other) {
            print("negotiate");
            if (other == null) {
                print("other ccm null in other found by " + cog.name);
            }
            foreach(ContractSpecification specification in negotiator.contractPreferencesAsOffererWith(other.cog)) {
                ContractSpecification rSpecification = other.amenable(cog, specification);
                if (rSpecification.exists()) {
                    print(cog.name + " is amenable");
                    return rSpecification;
                }
            }
            return ContractSpecification.NonExistant();
        }

        protected virtual ContractSpecification amenable(Cog other, ContractSpecification specification) {
            return negotiator.amenable(other, specification);
        }

        
        /*
        * Caller makes contract with itself as producer
        *  */
        public virtual void setup(ContractManager client, ContractSpecification specification) {
            if (!specification.offererIsProducer) {
                specification.offererIsProducer = true;
                client.setup(this, specification);
                return;
            }
            CogContract cc = new CogContract(
                this,
                client,
                specification.contractType,
                cog.producerActionsFor(client.cog, specification),
                client.cog.clientActionsFor(client.cog, specification),
                specification.connectionSiteAgreement);

            cc.unbreakable = cog.contractShouldBeUnbreakable(cc) || client.cog.contractShouldBeUnbreakable(cc);
            client.accept(cc, specification.connectionSiteAgreement);
            contractPortfolio.setContract(cc.connectionSiteAgreement.producerSite, cc);
            initiateContract(cc);
        }

        public static void initiateContract(CogContract cc) {
            cc.clientActions.receive(cc.producer.cog);
            cc.producerActions.initiate(cc.client.cog);
            cc.connectionSiteAgreement.connect();
        }

        protected virtual void accept(CogContract cc, ConnectionSiteAgreement csa) {
            contractPortfolio.setContract(csa.clientSite, cc);
        }

        public virtual void dissolve(CogContract cc) {
            print("dissolve");
            if (cc.unbreakable) {
                print("contract btwn: producer: " + cc.producer.cog.name + " and client: " + cc.client.cog.name + " was unbreakable.");
                return; }
            cc.producer.dissolveAsProducer(cc);
        }

        private void dissolveAsProducer(CogContract cc) {
            cc.client.forget(cc);
            cc.producerActions.dissolve(cc.client.cog);
            contractPortfolio.removeContract(cc);
        }

        protected virtual void forget(CogContract cc) {
            cc.clientActions.beAbsolvedOf(cc.producer.cog);
            contractPortfolio.removeContract(cc);
        }

        internal void getOutOfAll() {
            getOutOfAllExcept(new HashSet<ContractPortfolio.ClientTree.Node>());
        }

        internal void getOutOfAllExcept(HashSet<ContractPortfolio.ClientTree.Node> preserve) {
            foreach(CogContract cc in contractPortfolio) {
                Cog other = cc.getOtherParty(cog).cog;
                if (preserve.Contains(other.node)) { continue; }
                dissolve(cc);
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

        protected ContractPortfolio contractPortfolio {
            get { return cog.contractManager.contractPortfolio; }
        }

        public virtual ContractSpecification amenable(Cog other, ContractSpecification specification) {
            if (amenableFor(lookup.availabilty(other, specification.contractType), specification.offererIsProducer)) {
                return contractPortfolio.accommodatedSpecification(other, other.contractManager.contractPortfolio ,specification);
            }
            return ContractSpecification.NonExistant(); 
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
    protected abstract ConnectionSiteBoss getConnectionSiteBoss();
    
    public abstract ConnectionSiteAgreement.ConnektAction connektActionAsTravellerFor(ContractSpecification specification);
    
    protected virtual ContractNegotiator getContractNegotiator() {
        return new ContractNegotiator(this);
    }
    
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

    protected virtual bool contractShouldBeUnbreakable(CogContract contract) { return false; }


    private bool enterContractWith(Cog cog, bool permanent) {
        ContractSpecification spec = contractManager.negotiate(cog.contractManager);
        if(spec.contractType != CogContractType.NONEXISTENT) {
            contractManager.setup(cog.contractManager, spec);
            return true;
        }
        return false;
    }

    /*
    * Look-up whether--in theory--this cog could enter into
    * a contract with a given other cog, contract-type, role-type.
    * In theory because ViableContractLookup doesn't know 
    * whether there are actaully any ConnectionSites available (ConnectionSiteBoss knows about that question)
    * (CONSIDER: is VCL superfluous? Can CSB entirely take over VCL's job?)  
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
        return findAddOn(FindCog(other.transform));
    }

    public static AddOn findAddOn(Cog cog) {
        if (cog == null) { return null; }
        AddOn addOn = cog.GetComponentInParent<AddOn>();
        if (addOn == null) {
            IControllerAddOnProvider icaop = cog.GetComponentInParent<IControllerAddOnProvider>();
            if (icaop != null) {
                addOn = icaop.getControllerAddOn();
            }
        }
        return addOn;
    }

    #region ICursorAgentUrClient

    private DragHistory dragHistory;

    protected class DragHistory
    {
        public Vector3 start;
        public VectorXZ cursorStart;
        public VectorXZ lastCursor;
        public const float DisconnectRadius = 2f;
        public bool beyond(VectorXZ pos) {
            return DisconnectRadius * DisconnectRadius < (pos - cursorStart).magnitudeSquared;
        }
        public bool startedDisconnect;
        public bool nothingToDisconnect;

        public DragHistory(Vector3 start, VectorXZ cursorStart) {
            this.start = start;
            this.cursorStart = cursorStart;
            lastCursor = cursorStart;
        }

        public VectorXZ relativeToStart(VectorXZ global) {
            return global - start;
        }

        public VectorXZ relativeToCursorStart(VectorXZ global) {
            return global - cursorStart;
        }

        public VectorXZ updateLastCursorGetRelative(VectorXZ nextCursorGlobal) {
            VectorXZ result = nextCursorGlobal - lastCursor;
            lastCursor = nextCursorGlobal;
            return result;
        }

    }

    protected virtual bool vConnectTo(Collider other) {
        Cog cog = FindCog(other.transform);
        if (cog == null) { return false; }
        return enterContractWith(cog, false);
    }

    protected virtual void vDisconnect() {
        contractManager.getOutOfAllExcept(clientTree.root.children());
    }

    protected void drag(VectorXZ relative) {
        transform.position += relative.vector3();
    }
    

/*
 * interface methods 
 * */
    public bool connectTo(Collider other) {
        if (!dragHistory.nothingToDisconnect && !dragHistory.startedDisconnect) { return false; }
        Vector3 before = transform.position;
        bool result = vConnectTo(other);
        if (result) {
            clientTree.actionOnClientContractsBredthFirst(delegate (CogContract cc) {
                ContractManager.initiateContract(cc);
            });
        }
        return result;
    }

    public void normalDragStart(VectorXZ cursorPos) {
        //CONSIDER: more efficient to generate client tree only once per drag session?
        dragHistory = new DragHistory(transform.position, cursorPos);
        clientTree.highlight();
        dragHistory.nothingToDisconnect = !contractManager.hasAtleastOneContract;
    }

    public void normalDrag(VectorXZ cursorPos) {
        if (!dragHistory.nothingToDisconnect && !dragHistory.startedDisconnect && dragHistory.beyond(cursorPos)) {
           dragHistory.startedDisconnect = true;
           vDisconnect(); 
        }
        //TODO: replace actual parent childing with psuedo parent childing in parent child contracts 
        // it will de-head ache moving child sets
        clientTree.moveRelative(dragHistory.updateLastCursorGetRelative(cursorPos).vector3());
    }

    public void normalDragEnd(VectorXZ cursorPos) {
        clientTree.unhighlight();
        if (!dragHistory.nothingToDisconnect && !dragHistory.startedDisconnect) {
            clientTree.moveRelative(dragHistory.start - transform.position);
        }
    }

    #endregion;
}
