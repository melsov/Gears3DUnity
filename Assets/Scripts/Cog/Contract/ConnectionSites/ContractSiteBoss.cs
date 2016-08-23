using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class PairCTARSiteSet
{
    public readonly CTARSet ctarSet;
    public readonly SiteSet siteSet;
    internal bool isEmpty {
        get {
            return ctarSet.isEmpty;
        }
    }

    public PairCTARSiteSet(CTARSet ctarSet, SiteSet siteSet) {
        this.ctarSet = ctarSet;
        this.siteSet = siteSet;
    }

    public static PairCTARSiteSet emptyPair() {
        return new PairCTARSiteSet(CTARSet.emptyCTARSet(), SiteSet.emptySiteSet());
    }

    public static implicit operator KeyValuePair<CTARSet, SiteSet>(PairCTARSiteSet pair) {
        return new KeyValuePair<CTARSet, SiteSet>(pair.ctarSet, pair.siteSet);
    }

    public static implicit operator PairCTARSiteSet(KeyValuePair<CTARSet, SiteSet> keyValue) {
        return new PairCTARSiteSet(keyValue.Key, keyValue.Value);
    }

    public static PairCTARSiteSet fromSocketSet(Cog cog, SocketSet socketSet, RigidRelationshipConstraint rrc) {
        if (socketSet.sockets.Length == 0) { return emptyPair(); }
        SiteOrientation ori = SiteOrientation.fromRigidRelationshipConstraint(rrc);
        // ermmmmm...how to deal with rotation mode? (don't allow free rotations any more? and, instead, turn hinge joints into full-fledged cogs? <-- like)
        CTARSet tar = new CTARSet(new ContractTypeAndRole(
            CogContractType.PARENT_CHILD,
            rrc == RigidRelationshipConstraint.CAN_ONLY_BE_CHILD ? RoleType.CLIENT : RoleType.PRODUCER));
        List<ContractSite> temp = new List<ContractSite>(socketSet.sockets.Length);
        foreach (Socket s in socketSet.sockets) {
            temp.Add(new LocatableContractSite(cog, ori, s.transform, s.earMark));
        }
        SiteSet ss = new SiteSet(temp.ToArray());
        return new PairCTARSiteSet(tar, ss);
    }


}

public class ExclusionarySiteSetClientPair
{
    public readonly ClientOnlyCTARSet clientOnlyCTAR;
    public readonly ExclusionarySiteSet exclusionarySiteSet;

    public ExclusionarySiteSetClientPair(ClientOnlyCTARSet clientOnlyCTAR, ExclusionarySiteSet singleSiteSiteSet) {
        this.clientOnlyCTAR = clientOnlyCTAR;
        this.exclusionarySiteSet = singleSiteSiteSet;
    }

    public static implicit operator KeyValuePair<ClientOnlyCTARSet, ExclusionarySiteSet>(ExclusionarySiteSetClientPair pair) {
        return new KeyValuePair<ClientOnlyCTARSet, ExclusionarySiteSet>(pair.clientOnlyCTAR, pair.exclusionarySiteSet);
    }

    public static implicit operator ExclusionarySiteSetClientPair(KeyValuePair<ClientOnlyCTARSet, ExclusionarySiteSet> kvp) {
        return new ExclusionarySiteSetClientPair(kvp.Key, kvp.Value);
    }

    public static ExclusionarySiteSetClientPair fromSocketSet(Cog cog, SocketSet socketSet) {
        ClientOnlyCTARSet tar = new ClientOnlyCTARSet(CogContractType.PARENT_CHILD);
        List<ContractSite> sites = ContractSite.contractSiteListFromSocketSet(cog, socketSet);
        ExclusionarySiteSet ess = new ExclusionarySiteSet(sites.ToArray());
        return new ExclusionarySiteSetClientPair(tar, ess);
    }

}

public class ContractSiteBoss : IEnumerable<ContractSite>
{ 

    private readonly Dictionary<CTARSet, SiteSet> contractSites;

    public int Count {
        get {
            int result = 0;
            foreach(SiteSet ss in contractSites.Values) { result += ss.Length; }
            return result;
        }
    }

    public ContractSiteBoss(Dictionary<CTARSet, SiteSet> contractSites) {
        this.contractSites = contractSites;
        if (this.contractSites == null) {
            this.contractSites = new Dictionary<CTARSet, SiteSet>();
            MonoBehaviour.print("empty conn site boss");
        }
    }

    public static ContractSiteBoss emptyBoss() {
        return new ContractSiteBoss(new Dictionary<CTARSet, SiteSet>());
    }

    public void addSiteSet(KeyValuePair<CTARSet,SiteSet> pair) {
        contractSites.Add(pair.Key, pair.Value);
    }

    public SiteSet getSiteSet(ContractTypeAndRole specification) {
        foreach(CTARSet ctar in contractSites.Keys) {
            if (ctar.set.Contains(specification)) {
                MonoBehaviour.print("contains spec: " + specification.contractType);
                return contractSites[ctar];
            }
        }
        return new SiteSet(new ContractSite[0]);
    }

    public bool contains(ContractSite site) {
        return siteSetContaining(site);
    }

    private SiteSet siteSetContaining(ContractSite site) {
        foreach(SiteSet ss in contractSites.Values) { if (ss.Contains(site)) return ss; }
        return null;
    }

    public void setContract(ContractSite site, CogContract cc) {
        SiteSet ss = siteSetContaining(site);
        if (!ss) {
            Debug.LogError("Wha? trying to set a contract to a site that we don't own " + cc.ToString());
            return;
        }
        ss.setContract(site, cc);
    }

    public ContractSite siteHoldingContract(CogContract cc) {
        foreach(ContractSite site in this) {
            if (site.contract == cc) { return site; }
        }
        return null;
    }

    internal IEnumerable<SiteSet> getAllSiteSets() {
        return contractSites.Values;
    }

    internal ContractSite findEarmarkedSite(Earmark earmark) {
        foreach (ContractSite site in this) {
            if (EarmarkMatch.match(site.earmark, earmark)) {
                return site;
            }
        }
        return null;
    }

    public IEnumerator<ContractSite> GetEnumerator() {
        foreach(SiteSet ss in contractSites.Values) {
            foreach(ContractSite site in ss) { yield return site; }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

}

public class UniqueClientContractSiteBoss : ContractSiteBoss
{
    protected readonly ExclusionarySiteSet exclusionarySiteSet;
    public Cog uniqueProducer {
        get {
            return exclusionarySiteSet.site.contract.producer.cog;
        }
    }
    public ConnectionSiteAgreement uniqueConnectionSiteAgreement {
        get {
            return exclusionarySiteSet.site.contract.connectionSiteAgreement;
        }
    }

    public ContractSite producerSiteOfUniqueClientContractSite {
        get {
            return exclusionarySiteSet.site.contract.connectionSiteAgreement.producerSite;
        }
    }

    public bool isInContractWithProducer {
        get {
            return exclusionarySiteSet.site.contract != null;
        }
    }

    public UniqueClientContractSiteBoss(KeyValuePair<ClientOnlyCTARSet, ExclusionarySiteSet> uniqueClientSiteEntry) : 
        this(uniqueClientSiteEntry, new Dictionary<CTARSet, SiteSet>()) { }

    public UniqueClientContractSiteBoss(KeyValuePair<ClientOnlyCTARSet, ExclusionarySiteSet> uniqueClientSiteEntry, Dictionary<CTARSet, SiteSet> connectionSites) : base(connectionSites) {
        connectionSites.Add(uniqueClientSiteEntry.Key, uniqueClientSiteEntry.Value);
        exclusionarySiteSet = uniqueClientSiteEntry.Value;
    }

    public static UniqueClientContractSiteBoss emptyUBoss() {
        return new UniqueClientContractSiteBoss(new KeyValuePair<ClientOnlyCTARSet, ExclusionarySiteSet>(ClientOnlyCTARSet.emptySet(),ExclusionarySiteSet.emptySet()));
    }
}

public class SiteSet : IEnumerable<ContractSite>
{
    protected ContractSite[] sites;

    public SiteSet(ContractSite[] sites) {
        this.sites = sites;
        foreach(ContractSite s in sites) { UnityEngine.Assertions.Assert.IsTrue(s != null, "Site Set need non null connection sites"); }
    }

    public int Length {
        get { return sites.Length; }
    }

    public bool exacltyOneSite {
        get { return Length == 1; }
    }

    public bool Contains(ContractSite site) {
        foreach(ContractSite s in sites) {
            if (s == site) { return true; }
        }
        return false;
    }

    public virtual IEnumerator<ContractSite> GetEnumerator() {
        foreach(ContractSite site in sites) {
            if (site) {
                yield return site;
            }
        }
    }

    public static ProvisoPair Proviso(SiteSet offeree, SiteSet offerer) {
        List<ProvisoPair> provisos = OrderedProvisos(offeree, offerer);
        if (provisos.Count == 0) { return default(ProvisoPair); }
        return provisos[0];
    }

    private static List<ProvisoPair> OrderedProvisos(SiteSet offeree, SiteSet offerer) {
        List<ProvisoPair> result = new List<ProvisoPair>(12);
        foreach(ContractSite offereeSite in offeree) {
            if (offereeSite.occupied) { continue; }
            foreach(ContractSite offererSite in offerer) {
                if (offererSite.occupied) { continue; }
                if (!offereeSite.canAccommodate(offererSite)) { continue; }
                Debug.Log("adding");
                result.Add(new ProvisoPair(offererSite, offereeSite));
            }
        }
        Debug.Log("results: " + result.Count);
        return new List<ProvisoPair>(result).OrderBy(delegate (ProvisoPair pp) {
            return (pp.offereeSite.transform.position - pp.offererSite.transform.position).sqrMagnitude;
        }).ToList();
    }

    public List<ContractSite> sitesOrderedByDistanceFrom(VectorXZ pos) {
        return new List<ContractSite>(sites).OrderBy(delegate(ContractSite cs) {
            return (cs.transform.position - pos).magnitudeSquared;
        }).ToList();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
    
    public static SiteSet emptySiteSet() {
        return new SiteSet(new ContractSite[0]);
    }

    internal virtual void setContract(ContractSite site, CogContract cc) {
        site.contract = cc;
    }

    public SingleSiteSet GetSingleSiteSet() {
        if (!exacltyOneSite) { throw new Exception("trying to convert multi-site site set to single. what gives?"); }
        return new SingleSiteSet(sites[0]);
    }

    public static implicit operator bool(SiteSet ss) { return ss != null; }
}

/*
 * Allows only one contractSite to hold a contract at a time
 *  */
public class ExclusionarySiteSet : SiteSet
{
    private delegate ContractSite GetUniqueSite();
    private GetUniqueSite getNoSite = delegate () {
        return new NonExistantContractSite();
    };
    private GetUniqueSite getUniqueSite;

    public ExclusionarySiteSet(params ContractSite[] sites) : base(sites) {
        getUniqueSite = getNoSite;
    }

    public ContractSite site {
        get { return getUniqueSite(); }
    }

    private bool closed {
        get {
            foreach(ContractSite s in sites) { if (s.occupied) return true; }
            return false;
        }
    }

    internal override void setContract(ContractSite site, CogContract cc) {
        if (closed) { Debug.LogError(" site is closed. not setting"); return; }
        base.setContract(site, cc);
        getUniqueSite = delegate () { return site; };
    }

    public static ExclusionarySiteSet emptySet() {
        return new ExclusionarySiteSet(null);
    }
}

public class SingleSiteSet : SiteSet
{
    public SingleSiteSet(ContractSite site) : base(new ContractSite[] { site }) {}

    public ContractSite site {
        get { return sites[0]; }
    }
}



[System.Serializable]
public class ContractSite
{
    protected SiteOrientation _orientation;
    protected SiteOrientation orientation {
        get { return _orientation; }
        set { _orientation = value; }
    }

    private CogContract _contract;
    public virtual CogContract contract {
        get { return _contract; }
        set {
            _contract = value;
        }
    }

    public virtual Transform transform {
        get { return cog.transform; }
    }

    private Transform decoration;
    public void setDecoration(Transform decoration) {
        this.decoration = decoration;
        TransformUtil.AlignXZ(decoration, transform, null);
        decoration.position = TransformUtil.SetY(decoration.position, transform.position.y);
        decoration.parent = transform;
    }
    public void destroyDecoration() {
        if (!decoration) { return; }
        GameObject.Destroy(decoration.gameObject);
    }

    public bool occupied {
        get { return contract != null; }
    }

    public readonly Earmark earmark;
    private WeakReference _cog;

    public Cog cog {
        get { return (Cog)_cog.Target; }
    }

    public ContractSite(Cog cog_, SiteOrientation ori) : this(cog_, ori, Earmark.NONE) { }

    public ContractSite(Cog cog_) : this(cog_, SiteOrientation.selfMatchingOrientation(), Earmark.NONE) {
    }

    public ContractSite(Cog cog_, SiteOrientation orientation_, Earmark earMark) {
        _cog = new WeakReference(cog_);
        orientation = orientation_;
        this.earmark = earMark;
    }

    public bool canAccommodate(ContractSite other) {
        return orientation.canAlignWith(other.orientation);
    }

    public static implicit operator bool(ContractSite conSite) { return conSite != null; }

    public static ContractSite[] factory(Cog cog_, SiteOrientation ori_, int copies) {
        ContractSite[] sites = new ContractSite[copies];
        for(int i = 0; i < sites.Length; ++i) {
            sites[i] = new ContractSite(cog_, ori_, Earmark.NONE);
        }
        return sites;
    }

    public static List<ContractSite> contractSiteListFromSocketSet(Cog cog, SocketSet socketSet) {
        if (socketSet.sockets.Length == 0) { Debug.LogError("returning null UniSite"); return null; }
        SiteOrientation ori = SiteOrientation.fromRigidRelationshipConstraint(RigidRelationshipConstraint.CAN_ONLY_BE_CHILD);
        List<ContractSite> sites = new List<ContractSite>();
        foreach (Socket s in socketSet.sockets) {
            sites.Add(new LocatableContractSite(cog, ori, s.transform));
        }
        return sites;
    }

    #region debug

    private DebugIndicatorCube _debugIndicatorCube;
    public DebugIndicatorCube debugIndicatorCube {
        get {
            if (!_debugIndicatorCube) {
                _debugIndicatorCube = transform.GetComponentInChildren<DebugIndicatorCube>();
                if (!_debugIndicatorCube) {
                    _debugIndicatorCube = GameObject.Instantiate<DebugIndicatorCube>(Resources.Load<DebugIndicatorCube>("Prefabs/Debug/DebugIndicatorCube"));
                    UnityEngine.Assertions.Assert.IsFalse(_debugIndicatorCube == null, " still? ");
                    _debugIndicatorCube.transform.parent = transform;
                    _debugIndicatorCube.transform.localPosition = transform.localPosition * -.2f;
                }
            }
            return _debugIndicatorCube;
        }
    }
    #endregion
}

public class NonExistantContractSite : ContractSite
{
    public NonExistantContractSite() : base(null, SiteOrientation.alignsWithNothing(), Earmark.NONE) { }

    public override CogContract contract {
        get { return null; }
        set { }
    }
}


//TODO?????: ContractMultiSite allows for gears to have one contract slot for multiple SiteOrientations
// overrides 'canAccommodate()' to check all of its orientations against other's orientation
// ...wait maybe this is perverting the idea of a site 
// Solution: canAccomodate takes an explicitly non-multi type of ContractSite called 'ConnectionSite???'
// Multi-contract sites can expand themselves into arrays of non-multi contract/connection sites as needed

/*
LAMENT: the contract multi site seems to create a mess
For now, we're living with the situation where gear's have a backend socket but that socket isn't used as a contractSite.
Because gear's unique client site has to be used both for the socket and for other gear connections.
Basically, the situation is that sockets are sometimes used as contractsites and sometimes not (in the case of gears).
 *  */



public class LocatableContractSite : ContractSite
{
    protected Transform trans;
    public override Transform transform {
        get {
            return trans;
        }
    }

    public LocatableContractSite(Cog cog_, SiteOrientation ori, Transform site_) : this(cog_, ori, site_, Earmark.NONE) { }

    public LocatableContractSite(Cog cog_, SiteOrientation orientation_, Transform site_, Earmark earMark) : base(cog_, orientation_, earMark) {
        trans = site_;
    }

    public Vector3 position {
        get {
            if (!transform) { return VectorXZ.fakeNull.vector3(); }
            return transform.position;
        }
    }

    public static implicit operator Transform(LocatableContractSite connectionSite) { return connectionSite.transform; }

    public static void align(Transform targetSite, Transform destinationSite, Cog cog) {
        cog.move(cog.transform.position + destinationSite.position - targetSite.position);
        //cog.position += destinationSite.position - targetSite.position;
    }

    public static void alignAndPushYLayer(Transform targetSite, Transform destinationSite, Cog cog) {
        align(targetSite, destinationSite, cog);
        Debug.LogError("BFR:cog " + cog.name + " now has pos: " + cog.transform.position + " : rb : " + cog.rb.transform.position);
        float yTarget = destinationSite.transform.position.y + YLayer.LayerHeight * 5f;
        cog.move(TransformUtil.SetY(cog.transform.position, yTarget));
        Debug.LogError("AFT: cog " + cog.name 
            + " now has pos: " + cog.transform.position 
            + " : rb : " + cog.rb.transform.position + "target pos: " +
            yTarget);

        //cheat test
        cog.GetComponent<Rigidbody>().MovePosition(TransformUtil.SetY(cog.transform.position, yTarget));

    }

}

public enum Earmark { NONE, A, B, C, D, E, F, G, H, I, J, K, L, M };

public static class EarmarkMatch
{
    public static bool match(Earmark a, Earmark b) {
        if (a==Earmark.NONE) { return false; }
        return a == b;
    }
}