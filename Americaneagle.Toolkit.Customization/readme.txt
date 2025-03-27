This project contains all the base mappings for migrating a base K13 Accelerator site to base XbK Accelerator. Use this as a stating point for migrating a customized K13 site to a XbK.

Getting started:
You will want to work locally until the toolkit is configured to migrate all structured content. You will be dropping the XbK database regularly to start fresh. 
The /scripts directory contains tools for you to backup, restore and drop databases. 

Use the db-backup.sql to retrieve a backup of the XbK Accelerator base database. Retrieve a copy of the K13 production DB from servers.

Use the drop-db.ps1 to quickly drop the local XbK database. 
Use the restore-db.ps1 to quickly restore a fresh local XbK database.

------------------------------------------------------------------
REMEMBER TO UPDATE ALL CONNECTION STRING TO POINT TO YOUR LOCAL DB
------------------------------------------------------------------