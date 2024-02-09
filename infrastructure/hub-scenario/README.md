# Deploy 2 hub scenario

This bicep deploys 2 iot hubs with 1 dps service

Create your config file **main.bicepparam** with the following content:

```bicep
using 'main.bicep'

param location = 'northeurope'
param prefix = '<your prefix>'
```

To deploy:

```pwsh
az deployment sub create -n 2hubdeploy -l northeurope -f main.bicep -p main.bicepparam
```