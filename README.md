# iot-scale-demo
Demo repo for scaling azure iot scenarios 

**Disclaimer**: This is a demo repo and not intended for production use. 

## Generate test certificates using step

Use the following commands to generate test certificates using [step](https://smallstep.com/docs/step-cli/installation/). 

```pwsh
step ca init --deployment-type standalone --name huba --dns localhost --address 127.0.0.1:443 --provisioner HubAProvisoner --context huba
step ca init --deployment-type standalone --name hubb --dns localhost --address 127.0.0.1:443 --provisioner HubBProvisoner --context hubb
copy $Env:USERPROFILE/.step/authorities/huba/certs/intermediate_ca.crt -d ./certs/huba_intermediate.pem
copy $Env:USERPROFILE/.step/authorities/hubb/certs/intermediate_ca.crt -d ./certs/hubb_intermediate.pem
```



step certificate create client1-authnID client1-authnID.pem client1-authnID.key --ca .step/certs/huba_intermediate.crt --ca-key .step/secrets/intermediate_ca_key --no-password --insecure --not-after 2400h
