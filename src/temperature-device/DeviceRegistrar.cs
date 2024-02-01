using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TemperatureDevice;

public class DeviceRegistrar
{
    private readonly ClientCertificateGenerator _clientCertificateGenerator;
    private readonly ILogger<DeviceRegistrar> _logger;
    private readonly string? _globalDeviceEndpoint;
    private readonly string? _idScope;

    public DeviceRegistrar(ClientCertificateGenerator clientCertificateGenerator, IConfiguration configuration, ILogger<DeviceRegistrar> logger)
    {
        _clientCertificateGenerator = clientCertificateGenerator;
        _logger = logger;
        _globalDeviceEndpoint = configuration["GlobalDeviceEndpoint"];
        _idScope = configuration["IdScope"];
    }
    
    public async Task<TemperatureIotHubDevice> RegisterDeviceAsync(string deviceName, CancellationToken stoppingToken = default)
    {
        var certificate = _clientCertificateGenerator.GenerateClientCert(deviceName);
        using var security = new SecurityProviderX509Certificate(certificate);
        using var transport = new ProvisioningTransportHandlerMqtt();
        
        var provisioningDeviceClient = ProvisioningDeviceClient.Create(
            _globalDeviceEndpoint,
            _idScope,
            security,
            transport);
        try
        {
            var registrationResult = await provisioningDeviceClient.RegisterAsync(stoppingToken);
        
            if (registrationResult.Status != ProvisioningRegistrationStatusType.Assigned)
            {
                _logger.LogError("Registration status did not assign a hub, exiting...");
                throw new Exception("Registration status did not assign a hub, exiting...");
            }
        
            _logger.LogInformation("Device {DeviceId} registered to {AssignedHub} with {Status}", registrationResult.DeviceId, registrationResult.AssignedHub, registrationResult.Status);
        
            return new TemperatureIotHubDevice(certificate, registrationResult.DeviceId, registrationResult.AssignedHub);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error registering device {DeviceId}", deviceName);
            throw;
        }
    }
}