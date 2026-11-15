namespace NovaProject.Core.Infrastructure.Network;

public enum WirePacketType
{
    ClientHello = 1,
    ServerProof = 2,
    ClientAuthProof = 3,
    ServerAuthAck = 4,
    
    DirectMessage = 10,
    DeliveryError = 11
}

public record WireEnvelope
(
    WirePacketType Type,
    string PayloadJson
);

//STAGE 1: Anonymous client request
public record ClientHelloDto(byte[] Nonce);

//STAGE 2: Server proves its identity first
public record ServerProofDto(
    byte[] ServerNonce,
    byte[] ServerPublicKey,
    byte[] Signature
);

//STAGE 3: Client proves its own identity
public record ClientAuthProofDto(
    string RequestedUsername,
    byte[] ClientPublicKey,
    byte[] Signature
);

//STAGE 4: Server confirms identity and assigns an identity tag
public record ServerAuthAckDto(
    string ConfirmedUsername,
    string AssignedTag,
    bool Success,
    string? ErrorMessage = null
);
