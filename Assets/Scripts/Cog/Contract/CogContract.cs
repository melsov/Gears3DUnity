using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class CogContract
{
    private WeakReference _producer;
    private WeakReference _client;
    public CogContractType type;

    public Cog.ProducerActions producerActions;
    public Cog.ClientActions clientActions;

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

    public CogContract(Cog.ContractManager producer_, 
        Cog.ContractManager client_, 
        CogContractType _cct, 
        Cog.ProducerActions producerActions_, 
        Cog.ClientActions clientActions_) {
        this._producer = new WeakReference(producer_);
        this._client = new WeakReference(client_);
        type = _cct;
        producerActions = producerActions_;
        clientActions = clientActions_;
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

    public ContractSpecification(CogContractType type_, RoleType offerersRole_) {
        contractType = type_;
        offerersRole = offerersRole_;
    }

    public static ContractSpecification NonExistant() {
        return new ContractSpecification(CogContractType.NONEXISTENT, RoleType.UNINVOLVED);
    }
}


