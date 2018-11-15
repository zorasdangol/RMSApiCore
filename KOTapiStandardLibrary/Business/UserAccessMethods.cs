using Dapper;
using ImsPosLibraryCore.Helper;
using KOTAppClassLibrary.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace KOTapiStandardLibrary.Business
{
    public static class UserAccessMethods
    {

        public static string postuserVerification(User User)
        {
            string USERNAME = User.UserName;
            string PASSWORD = User.Password;
            string UNIQUEID = User.UniqueID;
            string encPassword;
            string key = "AmitLalJoshi";
            encPassword = GlobalClass.Encrypt(PASSWORD, key);
            using (SqlConnection cnMain = new SqlConnection(ConnectionDbInfo.ConnectionString))
            {
                cnMain.Open();
                try
                {
                    registerDevice(UNIQUEID, cnMain);
                    int i = cnMain.ExecuteScalar<int>("SELECT COUNT(*) FROM USERPROFILES WHERE UNAME='" + USERNAME + "' AND PASSWORD='" + encPassword + "'", cnMain);
                    if (i > 0)
                    {
                        i = cnMain.ExecuteScalar<int>("SELECT COUNT(*) FROM RMD_DEVICEVALIDATION WHERE UNIQUEID='" + UNIQUEID + "'", cnMain);
                        if (i > 0)
                        {
                            return "1";
                        }
                    }
                    return "0";
                }
                catch (Exception e)
                {
                    return e.Message;
                }
            }
        }

        public static  void registerDevice(string UNIQUEID, SqlConnection cnMain)
        {
            if (cnMain.ExecuteScalar<int>("SELECT COUNT(*) FROM RMD_DEVICEVALIDATION WHERE UNIQUEID = @UNIQUEID", new { UNIQUEID = UNIQUEID }) == 0)
                cnMain.Execute("INSERT INTO RMD_DEVICEVALIDATION (UNIQUEID, SALESTERMINAL,DIVISION,WAREHOUSE) SELECT TOP 1 @UNIQUEID DEVICE_ID, ST.NAME TERMINAL, @DIVISION DIVISION, W.NAME WAREHOUSE FROM SALESTERMINAL ST, RMD_WAREHOUSE W ORDER BY ISDEFAULT DESC", new { UNIQUEID = UNIQUEID, DIVISION = ConnectionDbInfo.DIVISION});
        }  
        
        public static string CheckAccess(User User)
        {
            string userName = User.UserName;
            string password = User.Password;

            string access = "0";
            string encPassword;
            string key = "AmitLalJoshi";
            encPassword = ConnectionDbInfo.Encrypt(password, key);
            using (SqlConnection cnMain = new SqlConnection(ConnectionDbInfo.ConnectionString))
            {
                cnMain.Open();
                //dt = new DataTable();
                try
                {
                    var dt = cnMain.ExecuteScalar<string>("SELECT AUTHORIZE FROM USERPROFILES WHERE UNAME='" + userName + "' AND PASSWORD='" + encPassword + "'", cnMain);
                    if(dt == null)
                    {
                        dt = "Incorrect username or password ";
                    }
                    return dt;
                }
                catch (Exception e)
                {
                    return e.Message;
                }
            }
        }

    }
}
