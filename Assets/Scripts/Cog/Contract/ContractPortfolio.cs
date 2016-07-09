using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

/*
 * Container for CogContracts and Connection Sites
 * Tells if any offered sites correspond-to/jive-with any of its open sites
 * */
public class ContractPortfolio : IEnumerable<CogContract>
{
    protected ConnectionSiteBoss connectionSiteBoss;
    protected WeakReference _cog;
    protected Cog cog {
        get { return (Cog)_cog.Target; }
    }

    public bool hasAtleastOneContract {
        get {
            foreach (SiteSet ss in connectionSiteBoss.getAllSites()) {
                foreach (ConnectionSite site in ss) {
                    if (site.occupied) return true;
                }
            }
            return false;
        }
    }

    public int validContractCount {
        get {
            int result = 0;
            foreach(SiteSet ss in connectionSiteBoss.getAllSites()) {
                foreach(ConnectionSite site in ss) {
                    if (site.occupied) result++;
                }
            }
            return result;
        }
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

    public IEnumerable<CogContract> contractsWithClients() {
        foreach(CogContract cc in this) {
            if (cc.client.cog == cog) { continue; }
            yield return cc;
        }
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
                    //rSpecification.connectionSiteAgreement.connektAction = rSpecification.connectionSiteAgreement.traveller.cog.connektActionAsTravellerFor(rSpecification);
                    return rSpecification;
                }
            }
        }
        return ContractSpecification.NonExistant();
    }

    #region ClientTree
    public class ClientTree
    {
        public Node root;
        
        public ClientTree(Node root) {
            this.root = root;
        }

        public delegate void CogAction(Cog cog);

        public void action(CogAction cogAction) {
            foreach(Node node in root.children()) {
                cogAction(node.portfolio.cog);
            }
        }

        public delegate void ContractAction(CogContract cc);

        public void actionOnClientContractsBredthFirst(ContractAction contractAction) {
            foreach(Node node in root.orderedChildrenBredthFirst()) {
                foreach(CogContract cc in node.portfolio.contractsWithClients()) {
                    contractAction(cc);
                }
            }
        }

        public void bredthFirstActionChildrenOnly(CogAction cogAction) {
            bredthFirstAction(cogAction, true);
        }

        public void bredthFirstAction(CogAction cogAction) {
            bredthFirstAction(cogAction, false);
        }

        private void bredthFirstAction(CogAction cogAction, bool skipRoot) {
            foreach(Node node in root.orderedChildrenBredthFirst()) {
                if (skipRoot && node == root) { continue; }
                cogAction(node.portfolio.cog);
            }
        }

        public void moveChildrenRelative(Vector3 nudge) {
            moveRelative(nudge, true);
        }

        public void moveRelative(Vector3 nudge) {
            moveRelative(nudge, false);
        }

        private void moveRelative(Vector3 nudge, bool excludeRoot) {
            foreach(Node node in root.children()) {
                if (excludeRoot && node == root) { continue; }
                node.portfolio.cog.transform.position += nudge;
            }
        }

        public void highlight() {
            highlight(true);
        }

        public void unhighlight() {
            highlight(false);
        }

        private void highlight(bool doHighlight) {
            foreach(Node node in root.children()) {
                Cog c = node.portfolio.cog;
                if (c && c.GetComponent<Highlighter>()) {
                    if (doHighlight) {
                        c.GetComponent<Highlighter>().highlight();
                    } else {
                        c.GetComponent<Highlighter>().unhighlight();
                    }
                }
            }
        }

        public class Node
        {
            protected WeakReference _portfolio;
            public ContractPortfolio portfolio {
                get { return (ContractPortfolio)_portfolio.Target; }
            }

            public Node(ContractPortfolio cp_) {
                _portfolio = new WeakReference(cp_);
            }

            public HashSet<Node> children() {
                HashSet<Node> result = new HashSet<Node>();
                children(ref result, false);
                return result;
            }

            //public HashSet<Node> nonParentChildChildren() {
            //    HashSet<Node> result = new HashSet<Node>();
            //    children(ref result, true, false);
            //    return result;
            //}

            private void children(ref HashSet<Node> childNodes, bool excludeParentChildContracts) {
                if (childNodes.Contains(this)) { return; }
                childNodes.Add(this);

                foreach (CogContract cc in portfolio) {
                    if (cc == null || cc.client == null || cc.client.cog == null) { continue; }
                    cc.client.cog.node.children(ref childNodes, excludeParentChildContracts);
                }
            }


            public Queue<Node> orderedChildrenBredthFirst() {
                Queue<Node> result = new Queue<Node>();
                Queue<Node> temp = new Queue<Node>();
                HashSet<Node> added = new HashSet<Node>();
                temp.Enqueue(this);
                added.Add(this);
                while(temp.Count > 0) {
                    Node next = temp.Dequeue();
                    foreach(CogContract cc in next.portfolio.contractsWithClients()) {
                        if (added.Contains(cc.client.cog.node)) { continue; }
                        temp.Enqueue(cc.client.cog.node);
                        added.Add(cc.client.cog.node);
                    }
                    result.Enqueue(next);
                }
                return result;
            }


        }
    }

    #endregion
}
