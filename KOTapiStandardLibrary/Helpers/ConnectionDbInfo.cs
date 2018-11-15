using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace KOTapiStandardLibrary.Helpers
{
    public class ConnectionDbInfo
    {
        public static string AppDataPath;
        private static string conDllName = "\\sqldbcon.dll";
        private static string _USER = "";
        private static string _PASSWORD = "";
        private static string _DATABASE = "";
        private static string _SERVER = "";
        private static string _TERMINAL = "";
        private static string _DIVISION = "";
        private static string _WAREHOUSE = "";
        private static string _CS;
        private static string _SaUser;
        private static string _SaPassword;
        private static string _connectionstring;
        private static string _printer;
        public static string USER { get { return _USER; } set { _USER = value; } }
        public static string PASSWORD { get { return _PASSWORD; } set { _PASSWORD = value; } }
        public static string DATABASE { get { return _DATABASE; } }
        public static string SERVER { get { return _SERVER; } }
        public static string TERMINAL { get { return _TERMINAL; } }
        public static string DIVISION { get { return _DIVISION; } }
        public static string WAREHOUSE { get { return _WAREHOUSE; } }
        public static string PRINTER { get; set; }
        public static bool DOBACKUP { get; set; }
        public static bool DODBUPDATE { get; set; }
        public static List<ConnectionModel> ConnectionList;
        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionstring) == true)
                {
                    //No database user ( Enabledbuser=0)
                    _connectionstring = GetConnectionString();
                }
                return _connectionstring;
            }
        }
        public static string SaConnectionString { get { return _CS; } }
        static ConnectionDbInfo()
        {
            //#if NET45
            //AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\IMS\\POS";
            AppDataPath = Environment.SystemDirectory ;            
            _CS = GetSaConnectionString();
//#endif
        }
        private static void JsonWrite()
        {
            try
            {
                string P = HexadecimalEncoding.ToHexString(Encrypt("tebahal"));
                JObject jobjFile = new JObject();
                jobjFile.Add("USER", "sa");
                jobjFile.Add("PASSWORD", P);
                jobjFile.Add("DATABASE", "CHYAPASA");
                jobjFile.Add("SERVER", "IMS007-PC\\TEBAHAL");
                jobjFile.Add("TERMINAL", "BBB");
                jobjFile.Add("DIVISION", "MMX");
                jobjFile.Add("WAREHOUSE", "MAIN STORE");
                jobjFile.Add("PRINTER", "");

                System.IO.File.WriteAllText(AppDataPath + conDllName, jobjFile.ToString());
            }
            catch (Exception ex) { }
        }

        private static bool GetDbConnectionInfo()
        {
            try
            {
                if (ConnectionList != null && ConnectionList.Count > 0)
                    return true;
                if (!File.Exists(AppDataPath + conDllName))
                {
                    JsonWrite();
                }

                string jsonStr = File.ReadAllText(AppDataPath + conDllName);
                if (!jsonStr.StartsWith("["))
                    jsonStr = "[" + jsonStr + "]";
                ConnectionList = JsonConvert.DeserializeObject<List<ConnectionModel>>(jsonStr);
                SetConnectionString(ConnectionList.First().COMPANYNAME);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static void SetConnectionString(string CompanyName)
        {
            try
            {
                var res = ConnectionList.Where(x => x.COMPANYNAME == CompanyName).DefaultIfEmpty(ConnectionList.First()).First();
                _SaUser = res.USER;
                _SaPassword = res.PASSWORD;
                _SERVER = res.SERVER;
                _DATABASE = res.DATABASE;
                _TERMINAL = res.TERMINAL;
                _DIVISION = res.DIVISION;
                _WAREHOUSE = res.WAREHOUSE;
                PRINTER = res.PRINTER;
                DOBACKUP = res.DOBACKUP;
                DODBUPDATE = res.DODBUPDATE;
                var ver = res.VERSION;
                if (ver != null)
                {
                    if (ver == "New")
                    {
                        _SaPassword = HexadecimalEncoding.FromHexString(res.PASSWORD);
                    }
                }
                _connectionstring = _CS = GetConnectionString();
            }
            catch (Exception ex)
            {
                //IMSErrorHandler.OnErrorCaught(new ImsExceptionArgs("SetConnectionString", ex.Message, ex));
            }
        }

        public static string GetConnectionString()
        {
            return GetSaConnectionString();
            //if enabledbuser==3  replace above code by below
            //SqlConnectionStringBuilder sbr = new SqlConnectionStringBuilder();
            //sbr.UserID = _USER;
            //sbr.Password = Encrypt(_PASSWORD);
            //sbr.InitialCatalog = DATABASE;
            //sbr.DataSource = SERVER;
            //return sbr.ConnectionString;
        }

        private static string GetSaConnectionString()
        {
            if (GetDbConnectionInfo())
            {
                SqlConnectionStringBuilder sbr = new SqlConnectionStringBuilder();
                sbr.UserID = _SaUser;               
                sbr.Password = (Encrypt(_SaPassword));
                sbr.InitialCatalog = DATABASE;
                sbr.DataSource = SERVER;
                return sbr.ConnectionString;
            }
            return "ERROR";
        }

        public static string CheckConnection()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    return "";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        //public static string Encrypt(string Text, string Key = "AmitLalJoshi")
        //{
        //    int i;
        //    string TEXTCHAR;
        //    string KEYCHAR;
        //    string encoded = string.Empty;
        //    for (i = 0; i < Text.Length; i++)
        //    {
        //        TEXTCHAR = Text.Substring(i, 1);
        //        var keysI = (i % (Key.Length - 1)) + (i < Key.Length - 1 ? 1 : 0);
        //        KEYCHAR = Key.Substring(keysI, Key.Length - keysI);
        //        var encrypted = Microsoft.VisualBasic.Strings.Asc(TEXTCHAR) ^ Microsoft.VisualBasic.Strings.Asc(KEYCHAR);
        //        encoded += Microsoft.VisualBasic.Strings.Chr(encrypted);
        //    }
        //    return encoded;
        //}
        public static string Encrypt(string txtValue, string Key = "AmitLalJoshi", bool toHex = false)
        {
            int i;
            string TextChar;
            string KeyChar;

            string retMsg = "";
            int ind = 1;

            for (i = 1; i <= Convert.ToInt32(txtValue.Length); i++)
            {
                TextChar = txtValue.Substring(i - 1, 1);
                ind = i % Key.Length;
                //if(ind==0)
                //{
                //KeyChar = Key.Substring(0);
                //}
                //else
                //{
                KeyChar = Key.Substring((ind));
                //}

                //Encrypted = Convert.ToInt32(Encoding.ASCII.(TextChar)) ^ Convert.ToInt32(Encoding.ASCII.GetBytes(KeyChar));
                byte str1 = Encoding.Default.GetBytes(TextChar)[0];
                byte str2 = Encoding.Default.GetBytes(KeyChar)[0];
                //Encrypted = str1 ^ str2;
                var encData = str1 ^ str2;
                retMsg = retMsg + Convert.ToChar(encData).ToString();
            }

            return retMsg;
        }


    }

    public class ConnectionModel
    {
        public string COMPANYNAME { get; set; } = "DEFAULT";
        public string USER { get; set; }
        public string PASSWORD { get; set; }
        public string DATABASE { get; set; }
        public string SERVER { get; set; }
        public string TERMINAL { get; set; }
        public string DIVISION { get; set; }
        public string WAREHOUSE { get; set; }
        public string PRINTER { get; set; } = "POS-76";
        public string VERSION { get; set; }
        public bool DOBACKUP { get; set; } = false;
        public bool DODBUPDATE { get; set; } = false;
    }
}
