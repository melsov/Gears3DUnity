using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LocatableSiteSetAndCTARSetSetup : MonoBehaviour {

    public LocationOrientation[] sites;
    public ContractTypeAndRole[] coTARs;

    public SiteSet getSiteSet() {
        return new SiteSet(siteList().ToArray());
    }

    private List<LocatableContractSite> siteList() {
        List<LocatableContractSite> result = new List<LocatableContractSite>();
        foreach(LocationOrientation lor in sites) {
            LocatableContractSite lcs = new LocatableContractSite(Cog.FindCog(transform), SiteOrientation.OrientedOrientation(lor.direction), lor.trans, lor.earmark);
            result.Add(lcs);
        }
        return result;
    }

    public ExclusionarySiteSet getExclusionarySiteSiteSet() {
        return new ExclusionarySiteSet(siteList().ToArray());
    }

    public CTARSet getCTARSet() {
        return new CTARSet(coTARs);
    }

    public ClientOnlyCTARSet getClientOnlyCTARSet() {
        List<CogContractType> temp = new List<CogContractType>();
        foreach(ContractTypeAndRole cotar in coTARs) {
            UnityEngine.Assertions.Assert.IsTrue(cotar.role == RoleType.CLIENT, "trying to make a client only CTAR set please help me");
            if(!temp.Contains(cotar.contractType))
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
        return new KeyValuePair<ClientOnlyCTARSet, ExclusionarySiteSet>(setup.getClientOnlyCTARSet(), setup.getExclusionarySiteSiteSet());
    }

}

[System.Serializable]
public struct LocationOrientation
{
    public CardinalDirection direction;
    public Transform trans;
    public Earmark earmark;
}
