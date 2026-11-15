using System.Security.Cryptography;

namespace NovaProject.RelayServer.Security;

public class ServerIdentityProvider
{
    private readonly ECDsa _serverKey;
    public byte[] PublicKey { get; }
    public string Fingerprint { get; }

    public ServerIdentityProvider(string keyFilePath = "server_identity.pem")
    {
        _serverKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        if (File.Exists(keyFilePath))
        {
            _serverKey.ImportFromPem(File.ReadAllText(keyFilePath));
        }
        else
        {
            File.WriteAllText(keyFilePath, _serverKey.ExportECPrivateKeyPem());
        }

        PublicKey = _serverKey.ExportSubjectPublicKeyInfo();
        Fingerprint = Convert.ToHexString(SHA256.HashData(PublicKey));
    }

    public byte[] SignChallenge(byte[] clientNonce)
    {
        return _serverKey.SignData(clientNonce, HashAlgorithmName.SHA256);
    }
}