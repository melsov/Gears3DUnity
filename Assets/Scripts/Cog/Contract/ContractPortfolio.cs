using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

/*
 * Container for CogContracts and Connection Sites
 * Tells if any offered sites correspond-to/jive-with any of its open sites
 * */
public class ContractPortfolio : IEnumerable<CogContract> {

    protected ConnectionSiteBoss connectionSiteBoss;
    protected WeakReference _cog;
    protected Cog cog {
        get { return (Cog)_cog.Target; }
    }
    public ContractPortfolio(Cog cog_, ConnectionSiteBoss connectionSiteBoss_) {
        _cog = new WeakReference(cog_);
        connectionSiteBoss = connectionSiteBoss_;
    }

    public void setContract(ConnectionSite site, CogContract contract) {
        if (!connectionSiteBoss.contains(site)) {
            Debug.LogError("Wha? trying to set a contract to a site that we don't own " + contract.ToString());
            return;
        }
        site.contract = contract;
    }

    public void removeContract(CogContract contract) {
        ConnectionSite site = connectionSiteBoss.siteHoldingContract(contract);
        if (site == null) {
            Debug.LogError("didn't find the site whose contract we wanted to remove. " + cog.name);
            return;
        }
        site.contract = null;
    }

    public IEnumerator<CogContract> GetEnumerator() {
        foreach(SiteSet ss in connectionSiteBoss.getAllSites()) {
            foreach (ConnectionSite site in ss) {
                if (site.occupied) {
                    yield return site.contract;
                } 
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public ContractSpecification accommodatedSpecification(Cog offerer, ContractPortfolio offerersPortfolio, ContractSpecification specification) {
        UnityEngine.Assertions.Assert.IsTrue(offerer != cog, "oh no, the offeree Cog is supposed to accommodate");
        SiteSet ss = connectionSiteBoss.getSites(offerer, specification.toContractTypeAndRoleForOfferee());
        foreach(ConnectionSite site in ss) {
            if (site.occupied) { continue; }
            foreach(ConnectionSite offerersSite in offerersPortfolio.connectionSiteBoss.getSites(cog, specification.toContractTypeAndRoleForOfferer())) { 
                if (site.canAccommodate(offerersSite)) {
                    ContractSpecification rSpecification = specification;
                    rSpecification.connectionSiteAgreement = new ConnectionSiteAgreement();
                    if (specification.offererIsProducer) {
                        rSpecification.connectionSiteAgreement.producerSite = offerersSite;
                        rSpecification.connectionSiteAgreement.clientSite = site;
                    } else {
                        rSpecification.connectionSiteAgreement.producerSite = site;
                        rSpecification.connectionSiteAgreement.clientSite = offerersSite;
                    }
                    rSpecification.connectionSiteAgreement.producerIsTraveller = specification.offererIsProducer;
                    rSpecification.connectionSiteAgreement.connektAction = rSpecification.connectionSiteAgreement.traveller.cog.connektActionAsTravellerFor(rSpecification);
                    return rSpecification;
                }
            }
        }
        return ContractSpecification.NonExistant();
    }
}
