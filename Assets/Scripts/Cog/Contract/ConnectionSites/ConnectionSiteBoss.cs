using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ConnectionSiteBoss { 

    private Dictionary<CTARSet, SiteSet> connectionSites;

    public ConnectionSiteBoss(Dictionary<CTARSet, SiteSet> connectionSites) {
        this.connectionSites = connectionSites;
        if (this.connectionSites == null) {
            this.connectionSites = new Dictionary<CTARSet, SiteSet>();
            MonoBehaviour.print("empty conn site boss");
        }
    }

//TODO: test Contract Specifcation lookup equality
    public SiteSet getSites(Cog offerer, ContractTypeAndRole specification) {
        foreach(CTARSet ctar in connectionSites.Keys) {
            MonoBehaviour.print("get sites: ctar");
            if (ctar.set.Contains(specification)) {
                MonoBehaviour.print("contains spec: " + specification.contractType);
                return connectionSites[ctar];
            }
        }
        return new SiteSet(new ConnectionSite[0]);
    }

    public bool contains(ConnectionSite site) {
        foreach(SiteSet ss in connectionSites.Values) {
            if (ss.Contains(site)) { return true; }
        }
        return false;
    }

    public ConnectionSite siteHoldingContract(CogContract cc) {
        foreach(SiteSet ss in connectionSites.Values) {
            foreach(ConnectionSite site in ss) {
                if (site.contract == cc) {
                    return site;
                }
            }
        }
        return null;
    }

    internal IEnumerable<SiteSet> getAllSites() {
        return connectionSites.Values;
    }
}

//TODO: ensure that we have a mechanism for instantiating ConnectionSites as needed

public class SiteSet : IEnumerable<ConnectionSite>
{
    private ConnectionSite[] sites;

    public SiteSet(ConnectionSite[] sites) {
        this.sites = sites;
        foreach(ConnectionSite s in sites) { UnityEngine.Assertions.Assert.IsTrue(s != null, "Site Set need non null connection sites"); }
    }

    public int Length {
        get { return sites.Length; }
    }

    public bool Contains(ConnectionSite site) {
        foreach(ConnectionSite s in sites) {
            if (s == site) { return true; }
        }
        return false;
    }

    public IEnumerator<ConnectionSite> GetEnumerator() {
        foreach(ConnectionSite site in sites) {
            if (site) {
                yield return site;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

}

[System.Serializable]
public class ConnectionSite
{
    private SiteOrientation orientation;
    //private WeakReference _contract = new WeakReference(null);
    public CogContract contract {
        get; // (CogContract)_contract.Target; }
        set; // { _contract =  new WeakReference(value); }
    }

    public bool occupied {
        get { return contract != null; }
    }

    private WeakReference _cog;
    public Cog cog {
        get { return (Cog)_cog.Target; }
    }
    
    public ConnectionSite(Cog cog_) : this(cog_, SiteOrientation.selfMatchingOrientation()) {
    }

    public ConnectionSite(Cog cog_, SiteOrientation orientation_) {
        _cog = new WeakReference(cog_);
        orientation = orientation_;
    }

    public bool canAccommodate(ConnectionSite other) {
        return orientation.canAlignWith(other.orientation);
    }

    public static implicit operator bool(ConnectionSite conSite) { return conSite != null; }

    public static ConnectionSite[] factory(Cog cog_, SiteOrientation ori_, int copies) {
        ConnectionSite[] sites = new ConnectionSite[copies];
        for(int i = 0; i < sites.Length; ++i) {
            sites[i] = new ConnectionSite(cog_, ori_);
        }
        return sites;
    }
}

public class LocatableConnectionSite : ConnectionSite
{
    private Transform site;

    public LocatableConnectionSite(Cog cog_, SiteOrientation orientation_, Transform site_) : base(cog_, orientation_) {
        site = site_;
    }

    public Vector3 position { get {
            if (!site) { return VectorXZ.fakeNull.vector3(); }
            return site.position;
        }
    }

    public static implicit operator Transform(LocatableConnectionSite connectionSite) { return connectionSite.site; }

    public static void align(LocatableConnectionSite targetSite, LocatableConnectionSite destinationSite, Transform target) {
        target.position += destinationSite.position - targetSite.position;
    }
}

