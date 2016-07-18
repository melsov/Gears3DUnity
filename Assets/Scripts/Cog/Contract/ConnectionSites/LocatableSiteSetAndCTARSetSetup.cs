using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LocatableSiteSetAndCTARSetSetup : MonoBehaviour {

    public LocationOrientation[] sites;
    public ContractTypeAndRole[] coTARs;

    public SiteSet getSiteSet() {
        List<LocatableContractSite> sites = new List<LocatableContractSite>();
        foreach(LocationOrientation lor in this.sites) {
            LocatableContractSite lcs = new LocatableContractSite(Cog.FindCog(transform), SiteOrientation.OrientedOrientation(lor.direction), lor.trans);
            sites.Add(lcs);
        }
        return new SiteSet(sites.ToArray());
    }

    public ExclusionarySiteSet getUniqueSiteSiteSet() {
        UnityEngine.Assertions.Assert.IsTrue(sites.Length == 1, "need exactly one site");
        LocatableContractSite lcs = new LocatableContractSite(Cog.FindCog(transform), SiteOrientation.OrientedOrientation(sites[0].direction), sites[0].trans);
        return new ExclusionarySiteSet(lcs);
    }

    public CTARSet getCTARSet() {
        return new CTARSet(coTARs);
    }

    public ClientOnlyCTARSet getClientOnlyCTARSet() {
        List<CogContractType> temp = new List<CogContractType>();
        foreach(ContractTypeAndRole cotar in coTARs) {
            UnityEngine.Assertions.Assert.IsTrue(cotar.role == RoleType.CLIENT, "trying to make a client only CTAR set please help me");
            temp.Add(cotar.contractType);
        }
        return new ClientOnlyCTARSet(temp.ToArray());
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

    public static KeyValuePair<ClientOnlyCTARSet, ExclusionarySiteSet> uniqueSiteSetAndClientOnlyCTARFor(Cog cog) {
        LocatableSiteSetAndCTARSetSetup setup = cog.GetComponentInChildren<LocatableSiteSetAndCTARSetSetup>();
        if (setup == null) {
            Debug.LogError(cog.name + " wanted locatable site setup but has no LSSandCTARSS. ");
            return new KeyValuePair<ClientOnlyCTARSet, ExclusionarySiteSet>(ClientOnlyCTARSet.emptySet(), ExclusionarySiteSet.emptySet());
        }
        return new KeyValuePair<ClientOnlyCTARSet, ExclusionarySiteSet>(setup.getClientOnlyCTARSet(), setup.getUniqueSiteSiteSet());
    }

}

[System.Serializable]
public struct LocationOrientation
{
    public CardinalDirection direction;
    public Transform trans;
}
