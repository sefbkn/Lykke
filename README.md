This project contains all items needed to integrate with the Decred network.

# dcrd-dcrdata

Contains docker containers and configurations files for dcrd and dcrdata.  The `Lykke.Service.Decred.Api` project requires both of these tools to be configured and running.

# Lykke.Service.Decred.Api

Provides interface to interact with the Decred network.  Reads settings from a location specified in the SettingsUrl environment variable.

Sample appsettings json

    {
      "ServiceSettings":{
        "NetworkType": "Test",
        "Dcrd":{
          "RpcEndpoint": "https://localhost:19109",
          "RpcUser": "user",
          "RpcPass": "pass"
        },
        "Asset":{
          "Precision": 8,
          "AssetId": "DCR",
          "Name": "Decred"
        },
        "Db":{ 
          "LogsConnString":"DefaultEndpointsProtocol=...",
          "azure":"DefaultEndpointsProtocol=...",
          "dcrdata":"Server=127.0.0.1;Port=5432;Database=;User Id=;Password=;"
        }
      },
      "SlackNotifications":{
        "AzureQueue":{
          "ConnectionString":"DefaultEndpointsProtocol=...",
          "QueueName":"slackout"
        }
      }
    }


# Lykke.Service.Decred.SignService

Provides interfaces to generate new Decred addresses and raw sign p2pkh transactions.  Reads settings from a location specified in the SettingsUrl environment variable.

Sample appsettings json

    {
      "NetworkType": "Test",
      "Logging": {
        "IncludeScopes": false,
        "Debug": {
          "LogLevel": {
            "Default": "Warning"
          }
        },
        "Console": {
          "LogLevel": {
            "Default": "Warning"
          }
        }
      }
    }
