using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

/*
 * Container for CogContracts and Connection Sites
 * Tells if any offered sites correspond-to/jive-with any of its open sites
 * */
public class ContractPortfolio : IEnumerable<CogContract>
{
    protected ContractSiteBoss contractSiteBoss;
    protected WeakReference _cog;
    protected Cog cog {
        get { return (Cog)_cog.Target; }
    }

    public bool hasAtleastOneContract {
        get {
            foreach (SiteSet ss in contractSiteBoss.getAllSiteSets()) {
                foreach (ContractSite site in ss) {
                    if (site.occupied) return true;
                }
            }
            return false;
        }
    }

    public bool contractedToProducer {
        get {
            foreach(CogContract cc in this) {
                if (cc.isClient(cog)) {
                    return true;
                }
            }
            return false;
        }
    }

    public int validContractCount {
        get {
            int result = 0;
            foreach(SiteSet ss in contractSiteBoss.getAllSiteSets()) {
                foreach(ContractSite site in ss) {
                    if (site.occupied) result++;
                }
            }
            return result;
        }
    }

    public ContractPortfolio(Cog cog_, ContractSiteBoss connectionSiteBoss_) {
        _cog = new WeakReference(cog_);
        contractSiteBoss = connectionSiteBoss_;
    }

    public void setContract(ContractSite site, CogContract contract) {
        contractSiteBoss.setContract(site, contract);
    }

//TEST whether exclusionary contracts are able to set new contracts
// really ever not be 'closed'
    public void removeContract(CogContract contract) {
        ContractSite site = contractSiteBoss.siteHoldingContract(contract);
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

    public IEnumerable<Cog> clients() {
        foreach(CogContract cc in contractsWithClients()) {
            yield return cc.client.cog;
        }
    }

    public IEnumerable<CogContract> contractsWithProducers() {
        foreach(CogContract cc in this) {
            if(cc.producer.cog == cog) { continue; }
            yield return cc;
        }
    }

    public IEnumerable<Cog> producers() {
        foreach(CogContract cc in contractsWithProducers()) {
            yield return cc.producer.cog;
        }
    }

    internal bool hasContractWith(Cog cog) {
        foreach(CogContract cc in this) {
            if (cc.isParticipant(cog)) {
                return true;
            }
        }
        return false;
    }

    public bool containsSite(ContractSite cs) {
        foreach (SiteSet ss in contractSiteBoss.getAllSiteSets()) {
            foreach (ContractSite site in ss) {
                if (site == cs) return true;
            }
        }
        return false;
    }


    public IEnumerator<CogContract> GetEnumerator() {
        foreach(SiteSet ss in contractSiteBoss.getAllSiteSets()) {
            foreach (ContractSite site in ss) {
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
        Assert.IsTrue(offerer != cog, "oh no, the offeree Cog is supposed to be the one accommodating");

        SiteSet ss = contractSiteBoss.getSiteSet(specification.contractTypeAndRoleForOfferee());
        SiteSet offererSiteSet = offerersPortfolio.contractSiteBoss.getSiteSet(specification.toContractTypeAndRoleForOfferer());

        ProvisoPair proviso = SiteSet.Proviso(ss, offererSiteSet);
        if (proviso) {
            return createContractSpecificationFrom(specification, proviso.offererSite, proviso.offereeSite);
        }
        Debug.Log("** got non existent");
        return ContractSpecification.NonExistant(); 

        /* pepperoni */
        foreach (ContractSite site in ss) {
            if (site.occupied) { continue; }
            foreach(ContractSite offerersSite in offererSiteSet.sitesOrderedByDistanceFrom(site.transform.position)) { // offerersPortfolio.contractSiteBoss.getSiteSet(specification.toContractTypeAndRoleForOfferer())) { 
                if (site.canAccommodate(offerersSite)) {
                    return createContractSpecificationFrom(specification, offerersSite, site);
                }
            }
        }
        Debug.Log("return non existent");
        return ContractSpecification.NonExistant();
    }

    private ContractSpecification createContractSpecificationFrom(ContractSpecification specification, ContractSite offerersSite, ContractSite site) {
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
        return rSpecification;
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

        public IEnumerator perMaxFixedFrameActionOnClients(ContractAction contractAction) {
            foreach (Node node in root.orderedChildrenBredthFirst()) {
                foreach (CogContract cc in node.portfolio.contractsWithClients()) {
                    float waitTime = Time.maximumDeltaTime * 1.1f;
                    yield return new WaitForSeconds(waitTime);
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
            foreach(Node node in root.immediateChildren()) {
                foreach(Node n in node.children()) {
                    move(n.portfolio.cog, nudge);
                }
            }
        }

        public void moveRelative(Vector3 nudge) {
            foreach(Node node in root.childrenForMoveOperation()) {
                move(node.portfolio.cog, nudge);
            } 
        }

        private static void move(Cog cog, Vector3 nudge) {
            cog.move(cog.transform.position + nudge);
        }

        public void moveRelatedAction(CogAction cogAction, bool excludeRoot) {
            foreach(Node node in root.childrenForMoveOperation()) {
                if (excludeRoot && node == root) { continue; }
                cogAction(node.portfolio.cog);
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

            private readonly bool propagatesMoveOperation; // = true;

            public Node(ContractPortfolio cp_) : this(cp_, true) { }

            public Node(ContractPortfolio cp_, bool propagatesMoveOperation) {
                _portfolio = new WeakReference(cp_);
                this.propagatesMoveOperation = propagatesMoveOperation;
            }

            public virtual HashSet<Node> childrenForMoveOperation() {
                HashSet<Node> result = new HashSet<Node>();
                childrenForMoveOperation(ref result);
                return result;
            }

            public void childrenForMoveOperation(ref HashSet<Node> childNodes) {
                if (childNodes.Contains(this)) { return; }
                childNodes.Add(this);
                if(!propagatesMoveOperation) { return; }

                foreach (CogContract cc in portfolio) {
                    if (cc == null || cc.client == null || cc.client.cog == null) { continue; }
                    cc.client.cog.node.childrenForMoveOperation(ref childNodes);
                }
            }

            public HashSet<Node> children() {
                HashSet<Node> result = new HashSet<Node>();
                children(ref result);
                return result;
            }

            private void children(ref HashSet<Node> childNodes) {
                if (childNodes.Contains(this)) { return; }
                childNodes.Add(this);

                foreach (CogContract cc in portfolio) {
                    if (cc == null || cc.client == null || cc.client.cog == null) { continue; }
                    cc.client.cog.node.children(ref childNodes);
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

            public IEnumerable<Node> immediateChildren() {
                foreach(CogContract cc in portfolio.contractsWithClients()) {
                    yield return cc.client.cog.node;
                }
            }


        }
    }

    #endregion
}

public struct ProvisoPair
{
    public readonly ContractSite offererSite;
    public readonly ContractSite offereeSite;

    public ProvisoPair(ContractSite offererSite, ContractSite offereeSite) {
        this.offereeSite = offereeSite;
        this.offererSite = offererSite;
    }

    public static implicit operator bool(ProvisoPair p) { return p.offereeSite && p.offererSite; }
}
