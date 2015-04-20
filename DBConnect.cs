using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Configuration;

namespace DBBackupUtility
{
    public class DbConnect
    {
        public OleDbConnection conn;
        //Externalize and read from xml file

        public SqlCommand SetConnection(string strConn)
        //Setting up Initial connection based on Input Server, DB details
        {
            try
            {
                //Log.Info(DbConnect.strConn);
                SqlCommand comm = new SqlCommand();
                comm.Connection = new SqlConnection(
                    strConn);
                return comm;
            }
            catch (Exception Ex)
            {

            }
            return null;
        }

        public void OpenConnection(SqlCommand comm)
        //Open DB Connection 
        {
            try
            {
                comm.Connection.Open();
            }
            catch (Exception Ex)
            {

            }
        }

        public void CloseConnection(SqlCommand comm)
        //Close DB Connection
        {
            try
            {
                comm.Connection.Close();
            }
            catch (Exception Ex)
            {

            }
        }
    }
}
