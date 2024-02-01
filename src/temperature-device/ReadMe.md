# Temperature Device

Simple temperature device that sends temperature data to a iothub or mqtt broker.

## Build docker images with certificates

```pwsh
docker build -t temperature-device -f Dockerfile .
```

## To run locally with debug

1. Create a file called 'appsettings.local.json' in the root of the project with the content: 
```json
{
  "GlobalDeviceEndpoint": "global.azure-devices-provisioning.net",
  "IdScope": "your id scope",
  "IntermediateCertFile": "path to certs\\huba_intermediate.pem",
  "IntermediateCertKey": "path to certs\\huba_intermediate.key",
  "IntermediateCertPasswordFile": "path to certs\\huba.pass",
  "NumberOfDevices": 1
}
```

## To run locally with docker

Copy the certificates (intermediate, key and pass file) you created to the certs folder **/certs** in the same directory as the dockerfile. The create a **.env** file in the same directory as the dockerfile with the following content:

```env
DPS_ID_SCOPE='your id scope'
```

To start run the following command:

```pwsh
docker compose build
docker compose up
```

To stop the containers run:

```pwsh
docker compose down
```
