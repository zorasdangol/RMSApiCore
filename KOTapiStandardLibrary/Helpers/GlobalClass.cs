using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace KOTapiStandardLibrary.Helpers
{
    public class GlobalClass
    {
        //public static string DataConnectionString;
        public static string Division = "MMX";
        public static string Terminal = "AAA";
        public static int GraceTime;
        public static string CompanyName;
        public static string CompanyAddress;
        public static decimal VAT = 13;
        public static Exception LastException;
        public static string GetTime = "(SELECT CONVERT(VARCHAR,(SELECT GETDate()),8))";
        public static string GetDate = "(SELECT GETDATE())";
        //public static string DataConnectionString { get { return ConfigurationManager.ConnectionStrings["DBSETTING"].ConnectionString; } }
        private static SqlConnection CnnMain;

        public static string Encrypt(string Text, string Key)
        {
            int i;
            string TEXTCHAR;
            string KEYCHAR;
            string encoded = string.Empty;
            for (i = 0; i < Text.Length; i++)
            {
                TEXTCHAR = Text.Substring(i, 1);
                var keysI = ((i + 1) % Key.Length);
                KEYCHAR = Key.Substring(keysI);
                var encrypted = Microsoft.VisualBasic.Strings.AscW(TEXTCHAR) ^ Microsoft.VisualBasic.Strings.AscW(KEYCHAR);
                encoded += Microsoft.VisualBasic.Strings.ChrW(encrypted);
            }
            return encoded;
        }

        public static DateTime GetAdDate(string BS)
        {
            try
            {
                DateTime AdDate;
                if (CnnMain.State == ConnectionState.Closed) CnnMain.Open();
                SqlCommand Cmd = new SqlCommand("Select AD from DATEMITI where MITI='" + BS + "'", CnnMain);
                using (SqlDataReader dr = Cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        AdDate = Convert.ToDateTime(dr["AD"]);
                        return AdDate;
                    }
                    else
                    {
                        throw new Exception(string.Format("Miti ({0}) is out of range.", BS));
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CnnMain.Close();
            }
        }

        public static string GetBSDate(DateTime Adate)
        {
            try
            {
                using (SqlConnection Con = new SqlConnection(Helpers.ConnectionDbInfo.ConnectionString))
                {
                    return Con.ExecuteScalar<string>("select dbo.dateToMiti('" + Adate.ToString("dd/MMM/yyyy") + "','/')");
                }
            }
            catch (Exception ex)
            {
                //IMSErrorHandler.OnErrorCaught(new ImsExceptionArgs("Globalclass.GetBSDate", ex.GetBaseException().Message, ex));
                return string.Empty;
            }
        }

        public static DateTime ServerDate()
        {
            using (SqlConnection con = new SqlConnection(ConnectionDbInfo.ConnectionString))
            {
                return con.ExecuteScalar<DateTime>("SELECT GETDATE()");
            }
        }
        public static string ServerTime()
        {
            using (SqlConnection con = new SqlConnection(ConnectionDbInfo.ConnectionString))
            {
                var date = con.Query("select GETDATE() as DATE").SingleOrDefault();
                return Convert.ToDateTime(date.DATE).ToString("hh:mm:ss tt");
            }
        }

    }
}
