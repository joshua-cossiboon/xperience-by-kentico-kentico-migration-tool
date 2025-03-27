# Parameters
[string]$DatabaseName = "" # Name of the database to drop
[string]$SqlServerInstance = ""    # SQL Server instance name


# Import SQL Server module if not already loaded
if (-not (Get-Module -ListAvailable -Name SqlServer)) {
    Import-Module SqlServer
}

# Check if the database exists
$databaseExistsQuery = "IF EXISTS (SELECT * FROM sys.databases WHERE name = '$DatabaseName') SELECT 1 ELSE SELECT 0"
$dbExists = Invoke-Sqlcmd -ServerInstance $SqlServerInstance -Query $databaseExistsQuery -TrustServerCertificate | Select-Object -ExpandProperty Column1

if ($dbExists -eq 1) {
    Write-Host "Database '$DatabaseName' exists. Proceeding with drop."

    ### DROP CONNECTIONS ###
    # Define the parameters
    $connectionString = "Server=$SqlServerInstance;Database=master;Integrated Security=True;TrustServerCertificate=true"

    # Create the SQL command to kill all connections
    $sqlCommand = @"
ALTER DATABASE [$DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
ALTER DATABASE [$DatabaseName] SET MULTI_USER;
"@

    # Execute the command using Invoke-Sqlcmd
    Invoke-Sqlcmd -Query $sqlCommand -ConnectionString $connectionString

    #### Drop the database
    $dropDatabaseQuery = "DROP DATABASE [$DatabaseName]"
    Invoke-Sqlcmd -ServerInstance $SqlServerInstance -Query $dropDatabaseQuery -TrustServerCertificate
    Write-Host "Database '$DatabaseName' has been dropped."

    # Remove associated files
    # Get the default data and log file locations
    $sqlQuery = "SELECT physical_name FROM sys.master_files WHERE database_id = DB_ID('$DatabaseName')"
    $filePaths = Invoke-Sqlcmd -ServerInstance $SqlServerInstance -Query $sqlQuery -TrustServerCertificate | Select-Object -ExpandProperty physical_name

    foreach ($filePath in $filePaths) {
        if (Test-Path $filePath) {
            Remove-Item -Path $filePath -Force
            Write-Host "File '$filePath' has been deleted."
        } else {
            Write-Host "File '$filePath' not found. No action needed."
        }
    }
} else {
    Write-Host "Database '$DatabaseName' does not exist. No action taken."
}
