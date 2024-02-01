using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;

namespace TemperatureDevice;

public class ClientCertificateGenerator
{
    private readonly string? _intermediateCertFile;
    private readonly string? _intermediateCertKey;
    private readonly string? _intermediateCertPasswordFile;
    
    public ClientCertificateGenerator(IConfiguration configuration)
    {
        _intermediateCertFile = configuration["IntermediateCertFile"];
        _intermediateCertKey = configuration["IntermediateCertKey"];
        _intermediateCertPasswordFile = configuration["IntermediateCertPasswordFile"];
        if (Directory.Exists("clientCerts"))
        {
            Directory.Delete("clientCerts", true);
        }
        Directory.CreateDirectory("clientCerts");
    }

    public X509Certificate2 GenerateClientCert(string clientAuthName)
    {
        SimpleExec.Command.Run("step", 
            $"certificate create {clientAuthName} ./clientCerts/{clientAuthName}.pem ./clientCerts/{clientAuthName}.key " +
            $"--ca {_intermediateCertFile} --ca-key {_intermediateCertKey} " +
            $"--ca-password-file {_intermediateCertPasswordFile} --no-password --insecure --not-after 2400h");
        
        var certificate = new X509Certificate2(
            X509Certificate2.CreateFromPemFile($"./clientCerts/{clientAuthName}.pem", $"./clientCerts/{clientAuthName}.key")
            .Export(X509ContentType.Pkcs12));
        return certificate;
    }
}