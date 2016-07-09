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
    public ConnectionSiteAgreement connectionSiteAgreement; // TODO: arrange so that csa's connektAction can be generated lazily, by asking the contract
// for the necessary info (in a new kind of struct?) the info is currently stored in ContractSpecification. Use this struct or define a newer (slimmer?) one.
// ultimate goal: be able to swap which party travels easily

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

    public ContractSpecification regenerateSpecificationForOfferee(Cog cog) {
        if (!valid) { return ContractSpecification.NonExistant(); }
        return new ContractSpecification(type, isClient(cog) ? RoleType.PRODUCER : RoleType.CLIENT);
    }
}

public class ConnectionSiteAgreement
{
    public ConnectionSite producerSite;
    public ConnectionSite clientSite;

    public ConnectionSite destination {
        get { return producerIsTraveller ? clientSite : producerSite; }
    }
    public ConnectionSite traveller {
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
            if (producerIsTraveller) {
                return producerSite.cog.connektActionAsTravellerFor(contract.regenerateSpecificationForOfferee(producerSite.cog));
            } else {
                return clientSite.cog.connektActionAsTravellerFor(contract.regenerateSpecificationForOfferee(clientSite.cog));
            }
        }
    }

    public static ConnektAction doNothing = delegate (ConnectionSiteAgreement csa) { };

    public static ConnektAction alignTarget(UnityEngine.Transform transform) {
        return delegate (ConnectionSiteAgreement csa) {
            LocatableConnectionSite.align((LocatableConnectionSite)csa.traveller, (LocatableConnectionSite)csa.destination, transform);
        };
    }

    public void connect() {
        connektAction(this);
    }
}

public class ConnectionSiteProffer
{
    public List<ConnectionSite> offerersSites;

    public ConnectionSiteProffer(params ConnectionSite[] args) {
        offerersSites = new List<ConnectionSite>(args);
    }
}

public enum CogContractType
{
    NONEXISTENT,
    DRIVER_DRIVEN, 
    CONTROLLER_ADDON_DRIVABLE,
    RECEIVER_ADDON_DRIVABLE,
    PARENT_CHILD
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
}

public struct CTARSet
{
    public HashSet<ContractTypeAndRole> set;

    public CTARSet(params ContractTypeAndRole[] args) {
        set = new HashSet<ContractTypeAndRole>();
        foreach(ContractTypeAndRole tar in args) {
            set.Add(tar);
        }
    }

    public static CTARSet clientDrivenAndChildSet() {
        return new CTARSet(
            new ContractTypeAndRole(CogContractType.DRIVER_DRIVEN, RoleType.CLIENT),
            new ContractTypeAndRole(CogContractType.PARENT_CHILD, RoleType.CLIENT)
            );
    }

    public static CTARSet fromCTAR(ContractTypeAndRole tar) {
        return new CTARSet(tar);
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
    public ContractTypeAndRole toContractTypeAndRoleForOfferee() {
        return new ContractTypeAndRole(contractType, offereesRole);
    }

    public ConnectionSiteAgreement connectionSiteAgreement;

    internal RoleType offereesRole {
        get { return offererIsProducer ? RoleType.CLIENT : RoleType.PRODUCER; }
    }

    //public ConnectionSiteProffer connectionSiteProffer;

    public ContractSpecification(CogContractType type_, RoleType offerersRole_) {
        contractType = type_;
        offerersRole = offerersRole_;
        connectionSiteAgreement = null;
    }

    public void setAdjoiningConnectionSite(ConnectionSite adjoiningSite) {
        throw new NotImplementedException();
    }

    public static ContractSpecification NonExistant() {
        return new ContractSpecification(CogContractType.NONEXISTENT, RoleType.UNINVOLVED);
    }

    public bool exists() {
        return contractType != CogContractType.NONEXISTENT && offereesRole != RoleType.UNINVOLVED;
    }

    //public static implicit operator bool(ContractSpecification cs) { return cs != default(ContractSpecification); }
}


