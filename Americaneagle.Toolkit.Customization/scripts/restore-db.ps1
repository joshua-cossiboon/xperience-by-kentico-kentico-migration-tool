
$serverInstance = "DP-1WS525\MSSQL16_DEV_2022"  # Example: "DP-1WS525\MSSQL16_DEV_2022"
$databaseName = "__migration_xbk_accelerator_dev"
$backupFile = "$PSScriptRoot\__migration_xbk_accelerator_dev.bak"

$sql = @"
RESTORE DATABASE [$databaseName]
FROM DISK = '$backupFile'
WITH REPLACE, RECOVERY;
"@

Invoke-Sqlcmd -ServerInstance $serverInstance -Query $sql -TrustServerCertificate
