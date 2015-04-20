using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;

namespace DBBackupUtility
{
    class Program
    {
        static void Main(string[] args)
        {
            String[] MachineIps = new String[] {"120.147.185.151"};
            String backupFolder = "";

            foreach (String machineIp in MachineIps)
            {
                string driveDetails = string.Format(@"\\{0}\{1}", machineIp, "c$");

                //Connect Drives
                RemoteDriveConnect RD = new RemoteDriveConnect(driveDetails, "Administrator", "", "password");
                RD.login(driveDetails, "Administrator", "", "password");

                //Delete Existing Backup
                //Directory.Delete(driveDetails + @"\" + "BackupFolder", true);

                //Create Backup Directory
                Directory.CreateDirectory(driveDetails + @"\" + "BackupFolder");

                ExecuteCommands EC = new ExecuteCommands(machineIp, "Administrator", "password");

                //Stop SQL Server
                //EC.StopSqlServer();

                //Start SQL Server
                //EC.StartSqlServer();

                backupFolder = string.Format("{0}{1}", @"C:\\", @"BackupFolder\Dbbackup.bak");
                EC.CreateBackup(machineIp, backupFolder, "InterfaceDB");

                //Delete Existing DB Directory
                //Directory.Delete(driveDetails + @"\" + "RestoreDatabases", true);

                //Create Backup Directory
                //Directory.CreateDirectory(driveDetails + @"\" + "RestoreDatabases");

                EC.RestoreBackup(machineIp, "InterfaceDB", backupFolder);

                RD.NetUseDelete();
            }
        }
    }
}
