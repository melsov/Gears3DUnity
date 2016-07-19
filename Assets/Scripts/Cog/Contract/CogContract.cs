using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class CogContract
{
    public bool unbreakable;

    private WeakReference _producer;
    private WeakReference _client;
    public CogContractType type;

    public Cog.ProducerActions producerActions;
    public Cog.ClientActions clientActions;
    public ConnectionSiteAgreement connectionSiteAgreement;

    public Cog.ContractManager producer {
        get {
            return (Cog.ContractManager)_producer.Target;
        }
    }

    public Cog.ContractManager client {
        get {
            return (Cog.ContractManager)_client.Target;
        }
    }

    public bool valid {
        get { return !(producer == null || producer.cog == null || client == null || client.cog == null); }
    }

    public bool isProducer(Cog cog) {
        if (cog == null || !valid) { return false; }
        return producer.cog == cog;
    }

    public bool isClient(Cog cog) {
        if (cog == null || !valid) { return false; }
        return client.cog == cog;
    }

    public bool isParticipant(Cog cog) {
        if (!cog || !valid) { return false; }
        return client.cog == cog || producer.cog == cog;
    }

    public Cog.ContractManager getOtherParty(Cog cog) {
        if (isClient(cog)) {
            return producer;
        }
        return client;
    }

    public CogContract(Cog.ContractManager producer_, 
        Cog.ContractManager client_, 
        CogContractType _cct, 
        Cog.ProducerActions producerActions_, 
        Cog.ClientActions clientActions_,
        ConnectionSiteAgreement connectionSiteAgreement_
        ) {
        this._producer = new WeakReference(producer_);
        this._client = new WeakReference(client_);
        type = _cct;
        producerActions = producerActions_;
        clientActions = clientActions_;
        this.connectionSiteAgreement = connectionSiteAgreement_;
    }

    public void suspend() {
        producerActions.suspend();
    }

    public void restore() {
        producerActions.restore();
    }

    public ContractSpecification regenerateSpecificationForOfferee(Cog cog) {
        if (!valid) { return ContractSpecification.NonExistant(); }
        return new ContractSpecification(type, isClient(cog) ? RoleType.PRODUCER : RoleType.CLIENT);
    }
}

public class ConnectionSiteAgreement
{
    public ContractSite producerSite;
    public ContractSite clientSite;

    public ContractSite destination {
        get { return producerIsTraveller ? clientSite : producerSite; }
    }
    public ContractSite traveller {
        get { return producerIsTraveller ? producerSite : clientSite; }
    }

    public virtual bool producerIsTraveller {
        get; set;
    }

    public void setTraveller(Cog cog) {
        if (producerSite.cog == cog) {
            producerIsTraveller = true;
        } else if (clientSite.cog == cog) {
            producerIsTraveller = false;
        }
    }

    protected CogContract contract {
        get { return clientSite.contract; }
    }

    public delegate void ConnektAction(ConnectionSiteAgreement csa);
    public ConnektAction connektAction {
        get {
            if (alwaysNothingAction) { return doNothing; }
            if (producerIsTraveller) {
                return producerSite.cog.connektActionAsTravellerFor(contract.regenerateSpecificationForOfferee(producerSite.cog));
            } else {
                return clientSite.cog.connektActionAsTravellerFor(contract.regenerateSpecificationForOfferee(clientSite.cog));
            }
        }
    }

    public static ConnektAction doNothing = delegate (ConnectionSiteAgreement csa) { };
    private bool alwaysNothingAction;

    public static ConnektAction alignTarget(UnityEngine.Transform transform) {
        return delegate (ConnectionSiteAgreement csa) {
            LocatableContractSite.align(csa.traveller.transform, csa.destination.transform, transform);
        };
    }

    public static ConnektAction alignAndPushYLayer(UnityEngine.Transform transform) {
        return delegate (ConnectionSiteAgreement csa) {
            LocatableContractSite.alignAndPushYLayer(csa.traveller.transform, csa.destination.transform, transform);
        };
    }

    public static ConnectionSiteAgreement GetConnectionSiteAgreement(ContractSite producerSite, ContractSite clientSite) {
        ConnectionSiteAgreement csa = new ConnectionSiteAgreement();
        csa.producerSite = producerSite;
        csa.clientSite = clientSite;
        return csa;
    }

    public static ConnectionSiteAgreement NoConnektActionConnectionSiteAgreement(ContractSite producerSite, ContractSite clientSite) {
        ConnectionSiteAgreement csa = GetConnectionSiteAgreement(producerSite, clientSite);
        csa.alwaysNothingAction = true;
        return csa;
    }

    public void connect() {
        connektAction(this);
    }
}

public enum CogContractType
{
    NONEXISTENT,
    DRIVER_DRIVEN, 
    CONTROLLER_ADDON_DRIVABLE,
    RECEIVER_ADDON_DRIVABLE,
    PARENT_CHILD, 
}

public enum RoleAvailablity
{
    UNAVAILABLE,
    CAN_ONLY_BE_PRODUCER,
    CAN_ONLY_BE_CLIENT,
    CAN_BE_PRODUCER_OR_CLIENT
}

public enum RoleType
{
    UNINVOLVED, PRODUCER, CLIENT
}

[System.Serializable]
public struct ContractTypeAndRole
{
    public CogContractType contractType;
    public RoleType role;
    public ContractTypeAndRole(CogContractType cct, RoleType r) {
        contractType = cct;
        role = r;
    }
    public override bool Equals(object obj) {
        if (obj is ContractTypeAndRole) {
            return Equals((ContractTypeAndRole)obj);
        }
        return base.Equals(obj);
    }
    public bool Equals(ContractTypeAndRole ctar) {
        return contractType == ctar.contractType && role == ctar.role;
    }
    public override int GetHashCode() {
        return base.GetHashCode();
    }

    public static ContractTypeAndRole GetParentChildCoTAR(RoleType rt) {
        return new ContractTypeAndRole(CogContractType.PARENT_CHILD, rt);
    }
}

public class CTARSet
{
    public HashSet<ContractTypeAndRole> set;
    internal bool isEmpty {
        get { return set.Count == 0; }
    }

    public CTARSet(params ContractTypeAndRole[] args) {
        set = new HashSet<ContractTypeAndRole>();
        foreach(ContractTypeAndRole tar in args) {
            set.Add(tar);
        }
    }

    public static CTARSet fromCTAR(ContractTypeAndRole tar) {
        return new CTARSet(tar);
    }

    public static CTARSet emptyCTARSet() {
        return new CTARSet();
    }
}

public class UniformRoleCTARSet : CTARSet
{
    public UniformRoleCTARSet(RoleType monoRoleType, params CogContractType[] args) : base(new ContractTypeAndRole[0]) { 
        foreach(CogContractType cct in args) {
            set.Add(new ContractTypeAndRole(cct, monoRoleType));
        }
    }

}

public class ClientOnlyCTARSet : UniformRoleCTARSet
{
    public ClientOnlyCTARSet(params CogContractType[] args) : base(RoleType.CLIENT, args) {}

    public static ClientOnlyCTARSet clientDrivenAndParentChildSet() {
        return new ClientOnlyCTARSet(CogContractType.PARENT_CHILD, CogContractType.DRIVER_DRIVEN);
    }

    public static ClientOnlyCTARSet drivenSet() {
        return new ClientOnlyCTARSet(CogContractType.DRIVER_DRIVEN);
    }

    public static ClientOnlyCTARSet emptySet() {
        return new ClientOnlyCTARSet();
    }
}


public struct ContractSpecification
{
    public CogContractType contractType;
    public RoleType offerersRole;
    public bool offererIsProducer {
        get { return offerersRole == RoleType.PRODUCER; }
        set {
            offerersRole = value ? RoleType.PRODUCER : RoleType.CLIENT;
        }
    }

    public ContractTypeAndRole toContractTypeAndRoleForOfferer() {
        return new ContractTypeAndRole(contractType, offerersRole);
    }
    public ContractTypeAndRole contractTypeAndRoleForOfferee() {
        return new ContractTypeAndRole(contractType, offereesRole);
    }

    public ConnectionSiteAgreement connectionSiteAgreement;

    internal RoleType offereesRole {
        get { return offererIsProducer ? RoleType.CLIENT : RoleType.PRODUCER; }
    }

    public ContractSpecification(CogContractType type_, RoleType offerersRole_) {
        contractType = type_;
        offerersRole = offerersRole_;
        connectionSiteAgreement = null;
    }

    public void setAdjoiningContractSite(ContractSite adjoiningSite) {
        throw new NotImplementedException();
    }

    public static ContractSpecification NonExistant() {
        return new ContractSpecification(CogContractType.NONEXISTENT, RoleType.UNINVOLVED);
    }

    public bool exists() {
        return contractType != CogContractType.NONEXISTENT && offereesRole != RoleType.UNINVOLVED;
    }

    public override string ToString() {
        return string.Format("Contract Specification: {0} : {1}", contractType, offerersRole);
    }

    //public static implicit operator bool(ContractSpecification cs) { return cs != default(ContractSpecification); }
}


