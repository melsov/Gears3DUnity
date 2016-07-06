using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LocatableSiteSetAndCTARSetSetup : MonoBehaviour {

    public LocationOrientation[] sites;
    public ContractTypeAndRole[] coTARs;

    public SiteSet getSiteSet() {
        List<LocatableConnectionSite> sites = new List<LocatableConnectionSite>();
        foreach(LocationOrientation lor in this.sites) {
            LocatableConnectionSite lcs = new LocatableConnectionSite(Cog.FindCog(transform), SiteOrientation.OrientedOrientation(lor.direction), lor.trans);
            sites.Add(lcs);
        }
        return new SiteSet(sites.ToArray());
    }

    public CTARSet getCTARSet() {
        return new CTARSet(coTARs);
    }
    
    public static Dictionary<CTARSet, SiteSet> connectionSiteLookupFor(Cog cog) {
        LocatableSiteSetAndCTARSetSetup[] setups = cog.GetComponentsInChildren<LocatableSiteSetAndCTARSetSetup>();
        if (setups.Length == 0) {
            Debug.LogError("no Locatable site set setup in " + cog.name + ". what gives?");
            return null;
        }
        Dictionary<CTARSet, SiteSet> lookup = new Dictionary<CTARSet, SiteSet>();
        foreach (LocatableSiteSetAndCTARSetSetup setup in setups) {
            lookup.Add(setup.getCTARSet(), setup.getSiteSet());
        }
        return lookup;
    }
    
}

[System.Serializable]
public struct LocationOrientation
{
    public CardinalDirection direction;
    public Transform trans;
}
