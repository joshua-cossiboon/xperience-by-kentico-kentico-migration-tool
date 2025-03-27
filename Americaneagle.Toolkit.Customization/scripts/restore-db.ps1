
$serverInstance = ""  # Example: "DP-1WS525\MSSQL16_DEV_2022"
$databaseName = ""
$backupFile = "$PSScriptRoot\BACKUPFILENAME.bak"

$sql = @"
RESTORE DATABASE [$databaseName]
FROM DISK = '$backupFile'
WITH REPLACE, RECOVERY;
"@

Invoke-Sqlcmd -ServerInstance $serverInstance -Query $sql -TrustServerCertificate
