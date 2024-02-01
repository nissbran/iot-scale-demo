# Deploy 2 hub scenario

This bicep deploys 2 iot hubs with 1 dps service

To deploy:

```pwsh
az deployment sub create -n 2hubdeploy -l northeurope -f main.bicep -p main.bicepparam
```