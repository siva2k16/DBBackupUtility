using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.Threading;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace DBBackupUtility
{
    class ExecuteCommands
    {
        private const string SQL_SERVICE_NAME = "MSSQLSERVER";
        private const string ServiceStoppedState = "Stopped";
        private const string ServiceRunningState = "Running";
        private const string WMI_NAMESPACE_PATH = "\\root\\cimv2";

        private string _machineIp;
        private string _user;
        private string _password;
        private String strConn = ConfigurationManager.AppSettings["DBConfigPath"];

        public ExecuteCommands(string MachineIp, string User, string Password)
        {
            this._machineIp = MachineIp;
            this._user = User;
            this._password = Password;
        }

        public void StopSqlServer()
        {
            ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Service  WHERE Name=\"" + SQL_SERVICE_NAME + "\"");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(CreateScope(), query);
            ManagementObjectCollection queryCollection = searcher.Get();
            foreach (ManagementObject sqlServerService in queryCollection)
            {
                sqlServerService.InvokeMethod("StopService", new object[] { });
                WaitOnState(searcher, ServiceStoppedState);
            }

        }

        private static void WaitOnState(ManagementObjectSearcher searcher, string state)
        {
            bool canReturn = false;
            do
            {
                ManagementObjectCollection queryCollection = searcher.Get();
                foreach (ManagementObject sqlServerService in queryCollection)
                {
                    canReturn = (string.Compare(sqlServerService["State"].ToString(), state, true) == 0);
                    if (!canReturn)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                    }
                }
            }
            while (!canReturn);
        }

        private ManagementScope CreateScope()
        {
            ManagementPath path = new ManagementPath();
            path.Server = this._machineIp;
            path.NamespacePath = WMI_NAMESPACE_PATH;
            ManagementScope scope = new ManagementScope(path);
            scope.Options.Username = this._user;
            scope.Options.Password = this._password;
            scope.Options.Timeout = TimeSpan.FromSeconds(10);
            scope.Connect();

            return scope;
        }

        public void StartSqlServer()
        {
            ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Service  WHERE Name=\"" + SQL_SERVICE_NAME + "\"");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(CreateScope(), query);
            ManagementObjectCollection queryCollection = searcher.Get();
            foreach (ManagementObject sqlServerService in queryCollection)
            {
                sqlServerService.InvokeMethod("StartService", new object[] { });
                WaitOnState(searcher, ServiceRunningState);
            }

        }

        public string CreateBackup(String machineIp, String getBackupFilePath, String dbName)
        {
            DbConnect Db = new DbConnect();
            List<string> sessionsList = new List<string>();
            //Prepare Connection String
            strConn = string.Format(strConn, machineIp);

            //Create Backup of tempdb 
            DbConnect dbcon = new DbConnect();

            try
            {
                SqlCommand comm = new SqlCommand();
                comm = dbcon.SetConnection(strConn);
                dbcon.OpenConnection(comm);
                comm.CommandType = CommandType.Text;
                //Always only one Active Store Present
                string sql = string.Format(@"USE MASTER 
                                            BACKUP DATABASE [{0}]  TO  DISK = N'{1}' WITH NOFORMAT, NOINIT,  "
                                            + "NAME = N'{0}-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10", dbName, getBackupFilePath);
                comm.CommandText = sql;
                comm.CommandTimeout = 3600;
                comm.ExecuteNonQuery();
                dbcon.CloseConnection(comm);
                return "0";
            }
            catch (Exception Ex)
            {
                return "-1";
            }
        }

        public string RestoreBackup(String machineIp, String DbName, String BakFilePath)
        {
            DbConnect Db = new DbConnect();
            List<string> sessionsList = new List<string>();

            //Prepare Connection String
            strConn = string.Format(strConn, machineIp);

            //Create Backup of tempdb 
            DbConnect dbcon = new DbConnect();

            try
            {
                SqlCommand comm = new SqlCommand();
                comm = dbcon.SetConnection(strConn);
                dbcon.OpenConnection(comm);
                comm.CommandType = CommandType.Text;
                string sql = string.Format("USE MASTER RESTORE DATABASE [{0}] " +
                                         "FROM DISK = N'{1}' WITH  FILE = 1,  NOUNLOAD,  REPLACE,  STATS = 10", DbName, BakFilePath);
                comm.CommandText = sql;
                comm.CommandTimeout = 3600;
                comm.ExecuteNonQuery();
                dbcon.CloseConnection(comm);
                return "0";
            }
            catch (Exception Ex)
            {
                return "-1";
            }
        }


    }
}
