# Troubleshooting application's access to CosmosDB

Get-AzADServicePrincipal -SearchString "RED RFID Exploit Database External"

Get-AzRoleDefinition | Where-Object { $_.Name -like "*Cosmos*" } | Format-Table -Property Name, IsCustom, Id

# Copied a custom role from duck.ai and placed it in:
# C:\Users\gladi\source\repos\red-rfid-aclj-project01SEP2025C\REDRFID\REDRFID\SupportingScripts\TroubleshootingCosmosDBInfo.ps1
