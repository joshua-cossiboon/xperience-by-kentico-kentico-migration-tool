-- Open this file in SSMS connected to the database you want to backup
-- update the database name
-- update TO DISK to your shared drive on the network
-- Run the command

BACKUP DATABASE [xbk_accelerator_dev] 
    TO DISK = N'\\ae.local\fileShares\Users\Shared\joshua.cossiboon\xbk_accelerator_dev.bak' WITH NOFORMAT, NOINIT, 
    NAME = N'xbk_accelerator_dev-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10