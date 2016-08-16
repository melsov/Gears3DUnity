using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;

/*
Base class for all mechanisms
*/
[System.Serializable]
public abstract class Cog : MonoBehaviour, ICursorAgentUrClient
{
    private ContractManager _contractManager;     
    private ContractManager contractManager {
        get {
            if (_contractManager == null) {
                _contractManager = new ContractManager(this, getContractNegotiator());
            }
            return _contractManager;
        }
    }
    private Pegboard __pegboard;
    protected Pegboard _pegboard {
        get {
            if (__pegboard == null) {
                __pegboard = GetComponentInChildren<Pegboard>();
                if (__pegboard == null) {
                    __pegboard = gameObject.AddComponent<Pegboard>();
                }
            }
            return __pegboard;
        }
    }        

    protected ContractPortfolio contractPortfolio {
        get {
            return contractManager.contractPortfolio;
        }
    }
    private ContractSiteBoss _connectionSiteBoss;
    protected ContractSiteBoss contractSiteBoss {
        get {
            if (_connectionSiteBoss == null) {
                _connectionSiteBoss = getContractSiteBoss();
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

    protected ContractPortfolio.ClientTree.Node _node;
    public virtual ContractPortfolio.ClientTree.Node node {
        get {
            if (_node == null) {
                _node = new ContractPortfolio.ClientTree.Node(contractPortfolio);
            }
            return _node;
        }
    }

    protected ContractPortfolio.ClientTree _clientTree;
    public virtual ContractPortfolio.ClientTree clientTree {
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

    #region move-rotate

    public Rigidbody rb;
    private void setupRB() {
        rb = GetComponent<Rigidbody>();
        if (!rb) {
            if (GetComponentInChildren<Rigidbody>()) {
                Debug.LogError(name + " has a child rigidbody but no top level rigidbody. This makes moving it problematic. \n DW: we'll just add one here");
                rb = gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }
    }
    public delegate void Move(Vector3 global);
    public Move move;
    public delegate void Rotate(Quaternion global);
    public Rotate rotate;
    private delegate VectorXZ GetXZPosition();
    private GetXZPosition getXZPosition;
    protected virtual void setupMoveAndRotate() {
        if (GetComponent<Rigidbody>()) {
            move = delegate (Vector3 global) {
                rb.MovePosition(global);
            };
            rotate = delegate (Quaternion global) {
                rb.MoveRotation(global);
            };
            getXZPosition = delegate () {
                return new VectorXZ(rb.position);
            };
        } else {
            move = delegate (Vector3 global) {
                transform.position = global;
            };
            rotate = delegate (Quaternion global) {
                transform.rotation = global;
            };
            getXZPosition = delegate () {
                return new VectorXZ(transform.position);
            };
        }
    }

    protected VectorXZ xzPosition { get { return getXZPosition(); } }

    #endregion
    protected ColliderSet colliderSet;
    protected NeighborColliderLookup neighborColliderLookup;

    public void Awake() {
        awake();
    }

    protected virtual void awake() {
        setupRB();
        setupMoveAndRotate();
        if (!colliderSet) {
            colliderSet = gameObject.AddComponent<ColliderSet>();
        }
        TransformUtil.AddComponentIfNot<Highlighter>(transform);
    }

    public virtual void Start() { }

    public virtual void OnEnable() { }

    public virtual void OnDisable() { }

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
            contractPortfolio = new ContractPortfolio(cog, cog.contractSiteBoss);
        }

        public bool hasContractWith(ContractManager other) {
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

        public ContractSpecification negotiateLimitOffer(ContractManager other, params ContractSpecification[] specs) {
            return negotiate(other, new List<ContractSpecification>(specs));
        }

        public ContractSpecification negotiate(ContractManager other) {
            return negotiate(other, negotiator.contractPreferencesAsOffererWith(other.cog));
        }

        private ContractSpecification negotiate(ContractManager other, List<ContractSpecification> specifications) {
            Bug.contractLog("negotiate");
            if (hasContractWith(other)) { return ContractSpecification.NonExistant(); }
            foreach(ContractSpecification specification in specifications) {
                print(cog.name + " offering: " + specification.ToString());
                ContractSpecification rSpecification = other.amenable(cog, specification);
                if (rSpecification.exists()) {
                    Bug.contractLog(other.cog.name + " is amenable");
                    return rSpecification;
                }
            }
            return ContractSpecification.NonExistant();
        }

        protected virtual ContractSpecification amenable(Cog other, ContractSpecification specification) {
            return negotiator.amenable(other, specification);
        }

        
        
        /*
        * Setup a contract. 
        *  */
        public void setup(ContractManager client, ContractSpecification specification) {
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
                client.cog.clientActionsFor(cog, specification),
                specification.connectionSiteAgreement);
            cc.unbreakable = cog.contractShouldBeUnbreakable(cc) || client.cog.contractShouldBeUnbreakable(cc);
            enter(client, cc);
        }

        private static void debugCubes(ContractSite producerSite, ContractSite clientSite, bool enteringInto) {
            Color c = new ColorRange(15, enteringInto ? Color.red : new Color(.3f, 0f, 0f)).random();
            producerSite.debugIndicatorCube.setColor(c);
            clientSite.debugIndicatorCube.setColor(c);
        }

        public void forceUnbreakable(ContractManager client, CogContract cc) {
            cc.unbreakable = true;
            enter(client, cc);
        }

        private void enter(ContractManager client, CogContract cc) {
            client.accept(cc); 
            contractPortfolio.setContract(cc.connectionSiteAgreement.producerSite, cc);
            debugCubes(cc.connectionSiteAgreement.producerSite, cc.connectionSiteAgreement.clientSite, true);
            initiateContract(cc);
        }

        public static void initiateContract(CogContract cc) {
            DEBUGTETHEREDCC(cc, "bfr");
            cc.clientActions.receive(cc.producer.cog);
            cc.producerActions.initiate(cc.client.cog);
            cc.connectionSiteAgreement.connect();
            DEBUGTETHEREDCC(cc, "AFT");
        }

        private static void DEBUGTETHEREDCC(CogContract cc, string msg) {
            if (cc.client.cog is Tethered.TetheredInput || cc.client.cog is Tethered.TetheredOutput) {
                Debug.Log(msg + "Prod: " + cc.producer.cog.name + " Cli: " + cc.client.cog.name);
                if (cc.producer.cog is Tethered.TetheredInput) {
                    Debug.LogError(cc.producer.cog.name + " set scalar is null? " + ((Tethered.TetheredInput)cc.producer.cog).debugSetScalarIsNull());
                }
                if (cc.client.cog is Tethered.TetheredOutput) {
                    Debug.LogError(cc.client.cog.name + " set scalar is null? " + ((Tethered.TetheredOutput)cc.client.cog).debugSetScalarIsNull());
                }
            }
            
        }

        protected virtual void accept(CogContract cc) { 
            contractPortfolio.setContract(cc.connectionSiteAgreement.clientSite, cc);
        }

        public virtual void dissolve(CogContract cc) {
            if (cc.unbreakable) {
                print("contract btwn: producer: " + cc.producer.cog.name + " and client: " + cc.client.cog.name + " was unbreakable.");
                return; }
            debugCubes(cc.connectionSiteAgreement.producerSite, cc.connectionSiteAgreement.clientSite, false);

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
                print("amenable for");
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
    protected abstract ContractSiteBoss getContractSiteBoss();
    
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
        private bool isFulfillSuspended;
        private ProducerAction suspendedFulfill;

        private static int doNothingTestPrint;
        private static readonly ProducerAction nothingAction = delegate (Cog cog) { if ((doNothingTestPrint++ % 40) == 0) print("producer nothing action for client " + cog.name); };

        public static ProducerActions getDoNothingActions() {
            ProducerActions pa = new ProducerActions();
            pa.initiate = nothingAction;
            pa.fulfill = nothingAction;
            pa.dissolve = nothingAction;
            pa.isFulfillSuspended = true;
            return pa;
        }

        public static ProducerAction getParentChildFulfillAction(Transform frontEnd, Transform childBackEnd, Quaternion deltaQuat) {
            return delegate (Cog _cog) {
                TransformUtil.AlignXZPushRotation(frontEnd, childBackEnd.position, deltaQuat, _cog);
            };
        }

        public void suspend() {
            suspendedFulfill = fulfill;
            fulfill = nothingAction;
            isFulfillSuspended = true;
        }
        
        public void restore() {
            if (!isFulfillSuspended) { return; }
            fulfill = suspendedFulfill;
            suspendedFulfill = nothingAction;
            isFulfillSuspended = false;
        }
    }

    public class ClientActions {
        public delegate void ClientAction(Cog other);
        public ClientAction receive;
        public ClientAction beAbsolvedOf;

        public static ClientActions getDoNothingActions() {
            ClientAction nothingAction = delegate (Cog cog) { Bug.contractLog("client nothing action for producer: " + cog.name); };
            ClientActions ca = new ClientActions();
            ca.receive = nothingAction;
            ca.beAbsolvedOf = nothingAction;
            return ca;
        }
    }

    protected virtual bool contractShouldBeUnbreakable(CogContract contract) { return false; }


    private bool enterContractWith(Cog cog, bool permanent) {
        Bug.contractLog(name + "enter contract with " + cog.name);
        ContractSpecification spec = contractManager.negotiate(cog.contractManager);
        if(spec.contractType != CogContractType.NONEXISTENT) {
            contractManager.setup(cog.contractManager, spec);
            return true;
        }
        Bug.contractLog(name + " unable to negotiate a contract" + cog.name);
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
            print(cog.name + " doesn't have a key for: " + cct);
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
    protected void forcePermanentEarmarkedParentChildContract(Cog child, Earmark earmark) {
        forcePermanentEarmarkedContract(child, earmark, CogContractType.PARENT_CHILD);
    }
    protected void forcePermanentEarmarkedContract(Cog client, Earmark earmark, CogContractType cct) {
        //CogContractType cct = CogContractType.PARENT_CHILD;
        ContractSite parentSite = contractSiteBoss.findEarmarkedSite(earmark);
        ContractSite childSite = client.contractSiteBoss.findEarmarkedSite(earmark);
        if (!parentSite || !childSite) {
            Debug.LogError(string.Format(" an earmarked site wasn't found: parent: {0} child {1} ", parentSite != null, childSite != null));
            return;
        }
        if (!childSite.canAccommodate(parentSite)) {
            Debug.LogError("child site cannot accommodate parent site");
            return;
        }
        Vector3 dif = Vector3.zero;
        if (cct == CogContractType.PARENT_CHILD && TransformUtil.IsDescendent(client.transform, transform)) {
            dif = client.transform.localPosition; // child.transform.position - transform.position;
            client.transform.SetParent(null);
            client.transform.position = transform.position + dif;
        }

        ConnectionSiteAgreement csa = ConnectionSiteAgreement.NoConnektActionConnectionSiteAgreement(parentSite, childSite);
        ContractSpecification fakeishSpecification = new ContractSpecification(cct, RoleType.PRODUCER);
        ProducerActions prodActions = producerActionsFor(client, fakeishSpecification);
        prodActions.fulfill = ProducerActions.getParentChildFulfillAction(parentSite.transform, childSite.transform, Quaternion.identity);
        ClientActions cliActions = client.clientActionsFor(this, fakeishSpecification);
        CogContract cc = new CogContract(
            contractManager, 
            client.contractManager, 
            cct, 
            prodActions,
            cliActions,
            csa);
        contractManager.forceUnbreakable(client.contractManager, cc);
    }

    protected static void addConnectionSiteEntriesForBackSocketSet(Cog cog, ContractSiteBoss csb) {
        PairCTARSiteSet pair = PairCTARSiteSet.fromSocketSet(cog, cog._pegboard.getBackendSocketSet(), RigidRelationshipConstraint.CAN_ONLY_BE_CHILD);
        if (pair.isEmpty) { return; }
        csb.addSiteSet(pair);
    }

    protected static void addConnectionSiteEntriesForFrontSocketSet(Cog cog, ContractSiteBoss csb) {
        PairCTARSiteSet pair = PairCTARSiteSet.fromSocketSet(cog, cog._pegboard.getFrontendSocketSet(), RigidRelationshipConstraint.CAN_ONLY_BE_PARENT);
        if (pair.isEmpty) { return; }
        csb.addSiteSet(pair);
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

    public DragHistory dragHistory;

    public class DragHistory
    {
        public readonly Vector3 start;
        public readonly VectorXZ cursorStart;
        public readonly VectorXZ cursorRelative;
        //public VectorXZ lastCursor;
        public const float DisconnectRadius = 2f;
        public bool beyond(VectorXZ pos) {
            return DisconnectRadius * DisconnectRadius < (pos - cursorStart).magnitudeSquared;
        }
        public bool startedDisconnect;
        //public bool somethingToDisconnect;
        public bool wasContractedToAProducer;

        public DragHistory(Vector3 start, VectorXZ cursorStart) {
            this.start = start;
            this.cursorStart = cursorStart;
            cursorRelative = cursorStart - start;
            //lastCursor = cursorStart;
        }

        public VectorXZ relativeToStart(VectorXZ global) {
            return global - start;
        }

        public VectorXZ relativeToCursorStart(VectorXZ global) {
            return global - cursorStart;
        }

        public VectorXZ updateLastCursorGetRelative(Vector3 cogPos, VectorXZ cursorGlobal) {
            VectorXZ result = cursorGlobal - cursorRelative;
            return result - cogPos;
        }

    }

    protected virtual bool vConnectTo(Collider other) {
        Bug.contractLog(name + " vConn");
        Cog cog = FindCog(other.transform);
        if (cog == null || cog == this) { return false; }
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
        if (dragHistory == null) {
            return false; //sometimes use this without dragging??
        }
        if (dragHistory.wasContractedToAProducer && !dragHistory.startedDisconnect) { return false; }
        Vector3 before = transform.position;
        bool result = vConnectTo(other);
        if (result) {
            StartCoroutine(clientTree.perMaxFixedFrameActionOnClients(delegate(CogContract cc) { 
                ContractManager.initiateContract(cc);
            }));
        }
        return result;
    }

    public virtual void normalDragStart(VectorXZ cursorPos) {
        //CONSIDER: more efficient to generate client tree only once per drag session?
        dragHistory = new DragHistory(transform.position, cursorPos);
        clientTree.highlight();
        dragHistory.wasContractedToAProducer = contractPortfolio.contractedToProducer;
        Bug.contractLog(name + " contracted to a producer: " + dragHistory.wasContractedToAProducer);
        if (nonKinematicWhilePositioning) {
            collectRBsAndUnsetKinematic();
        }
        suspendOnDragStart();

        clientTree.bredthFirstAction(delegate (Cog cog) {
            cog.onProducerWillStartDrag();
        });
    }

    public virtual void normalDrag(VectorXZ cursorPos) {
        if (dragHistory.wasContractedToAProducer && !dragHistory.startedDisconnect && dragHistory.beyond(cursorPos)) {
           dragHistory.startedDisconnect = true;
           vDisconnect(); 
        }
        clientTree.moveRelative(dragHistory.updateLastCursorGetRelative(transform.position, cursorPos).vector3());
    }

    public virtual void normalDragEnd(VectorXZ cursorPos) {
        clientTree.unhighlight();
        resetNonKinematicRBs();

        clientTree.bredthFirstAction(delegate (Cog client) {
            client.onProducerWillEndDrag();
        });

        if (dragHistory.wasContractedToAProducer && !dragHistory.startedDisconnect) {
            clientTree.moveRelative(dragHistory.start - transform.position);
            restoreOnDragEnd();
        }
    }

    [SerializeField]
    protected bool nonKinematicWhilePositioning = true;
    private HashSet<Rigidbody> nonKinematicRBs = new HashSet<Rigidbody>();

    private void collectRBsAndUnsetKinematic() {
        nonKinematicRBs.Clear();
        foreach(Rigidbody rb in GetComponentsInChildren<Rigidbody>()) {
            if (!rb.isKinematic) {
                rb.isKinematic = true;
                nonKinematicRBs.Add(rb);
            }
        }
    }
    private void resetNonKinematicRBs() {
        foreach(Rigidbody rb in nonKinematicRBs) {
            rb.isKinematic = false;
        }
    }

    public virtual void onProducerWillStartDrag() { }
    public virtual void onProducerWillEndDrag() { }

    protected virtual void suspendOnDragStart() { }
    protected virtual void restoreOnDragEnd() { }

    #endregion;

    #region debug
    private Vector3 _OLDPOS = VectorXZ.fakeNull.vector3();
    private Vector3 OLDPOS {
        get {
            if (_OLDPOS.Equals(VectorXZ.fakeNull.vector3())) { _OLDPOS = transform.position; }
            return _OLDPOS;
        }
        set { _OLDPOS = value; }
    }

    public void DEBUGPOSCHANGE<T>() where T : MonoBehaviour {
        DEBUGPOSCHANGE<T>(string.Format("--POS CHANGE FOR: {0}--", name));
    }

    private void DEBUGPOSCHANGE<T>(string msg) where T : MonoBehaviour {
        if (!(this is T)) { return; }
        if (!OLDPOS.Equals(transform.position)) {
            Debug.LogError(msg);
            OLDPOS = transform.position;
        }
    }
    #endregion
}
