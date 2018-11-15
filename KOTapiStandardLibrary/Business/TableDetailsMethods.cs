using Dapper;
using ImsPosLibraryCore.Helper;
using KOTAppClassLibrary.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace KOTapiStandardLibrary.Business
{
    public static class TableDetailsMethods
    {
        static string QUERY_RMD_KOTMAIN_STATUS = "INSERT INTO RMD_KOTMAIN_STATUS(KOTID,TABLENO,STATUS,REMARKS,Division,edate) VALUES (@KOTID,@TABLENO,@STATUS,@REMARKS,'" + GlobalClass.DIVISION + "',getdate())";
        static string InsertQueryForKotPrint = "insert into printkot(KOTID,TABLENO,DESCA,MENUCODE,ISBOT,QUANTITY,REMARKS,SNO,UNIT,MCODE,TRNDATE,KOTTIME,KOT,USERNAME,PAX,ComboItem, REFSNO) values(@KOTID,@TABLENO,@DESCA,@MENUCODE,@ISBOT,@QUANTITY,@REMARKS,@SNO,@UNIT,@MCODE,@TRNDATE,@KOTTIME,@KOT,@USERNAME,@PAX,@ComboItem, @REFSNO)";

        public static String getdayCloseTable()
        {
            try
            {
                //return "0";
                using (SqlConnection cnMain = new SqlConnection(ConnectionDbInfo.ConnectionString))
                {
                    DateTime ServerDate = cnMain.ExecuteScalar<DateTime>("SELECT GETDATE()");
                    TimeSpan KotActiveTime = cnMain.ExecuteScalar<TimeSpan>("SELECT ISNULL(KotActiveTime, '00:00:00.000') FROM SETTING");
                    foreach (DateTime KotTime in cnMain.Query<DateTime>("SELECT TRNDATE + CAST(TRNTIME AS DATETIME) FROM RMD_KOTMAIN KM JOIN RMD_KOTMAIN_STATUS KMS ON KM.KOTID=KMS.KOTID WHERE KMS.STATUS='ACTIVE'"))
                    {
                        DateTime KotAllowdTime = ServerDate.Date.Add(KotActiveTime);
                        if (ServerDate > KotAllowdTime && KotTime < KotAllowdTime)
                        {
                            return "1";
                        }
                    }
                    return "0";
                    //return (cnMain.ExecuteScalar<int>("SELECT COUNT(TABLENO) FROM RMD_KOTMAIN WHERE TRNDATE < CONVERT(VARCHAR, GETDATE(), 101)") > 0) ? "1" : "0";
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public static string CancelOrder(string tableNo, string user, string remarks)
        {
            try
            {
                string cancelRemarks = remarks;
                              

                using (SqlConnection con = new SqlConnection(ConnectionDbInfo.ConnectionString))
                { 
                    var Q = string.Format
                                   (
                                       @"SELECT MCODE, ItemDesc DESCA, ISNULL(SUM(Quantity),0) QUANTITY, MAX(KOTTIME) KOTTIME, MAX(UNIT) UNIT,
                                        CASE WHEN K.Remarks='' THEN 'No Remarks' ELSE K.REMARKS END AS REMARKS, 
                                        CAST(ISBOT AS VARCHAR) IsBarItem, CAST(SNO AS VARCHAR) SNO,ISNULL(MAX(REFSNO),0) REFSNO, MAX(KOT) KOT FROM RMD_KOTPROD K JOIN RMD_KOTMAIN_STATUS KMS ON KMS.KOTID=K.KOTID WHERE KMS.STATUS='ACTIVE' AND K.TABLENO ='{0}'  
                                        GROUP BY MCODE, ItemDesc, K.Remarks, ISBOT, SNo", tableNo
                                   );
                    var list = con.Query<KOTProd>(Q).ToList();
                    ObservableCollection<KOTProd> tempList = new ObservableCollection<KOTProd>();

                    
                    var KOTID = con.ExecuteScalar<int>("SELECT  KOTID FROM RMD_KOTMAIN_STATUS  WHERE STATUS = 'ACTIVE' AND TABLENO = '" + tableNo + "'");


                    byte ps = con.ExecuteScalar<byte>("SELECT PREBILL_STATUS FROM RMD_KOTMAIN_STATUS WHERE KOTID=" + KOTID);
                    string RPN = con.ExecuteScalar<string>("SELECT REF_PREBILL_NOS FROM RMD_KOTMAIN_STATUS WHERE KOTID=" + KOTID);
                    if (ps == 1)
                    {
                        return ("Pre Bill Detected...");
                    }
                    else if (!string.IsNullOrEmpty(RPN))
                    {
                        return "Pre Bill Detected...";
                    }
                    else
                    {
                        string role = con.ExecuteScalar<string>("select role from userProfiles where uname='" + user + "'");

                        byte PrebillStatus = con.ExecuteScalar<byte>("SELECT ISNULL(MIN(STATUS), 128) FROM RMD_KOTMAIN_PREBILL WHERE TABLENO = '" + tableNo + "' AND KOTID = " + KOTID);
                        if (role == "A" && PrebillStatus == 128)
                        {
                            foreach (var l in list.ToList())
                            {

                                if (tempList.FirstOrDefault(x => x.MCODE == l.MCODE) == null)
                                {
                                    var SList = list.Where(w => w.MCODE == l.MCODE);
                                    var sumQty = SList.Sum(x => x.Quantity);
                                    if (sumQty > 0)
                                    {
                                        l.Quantity = -sumQty;
                                        tempList.Add(l);
                                    }
                                }

                            }
                            con.Open();
                            SqlTransaction tran = con.BeginTransaction();
                            try
                            {
                                var kpO = con.Query<KotMain>("select KM.* from rmd_kotmain KM JOIN RMD_KOTMAIN_STATUS KMS ON KM.KOTID=KMS.KOTID where KM.KOTID=" + KOTID, null, tran).SingleOrDefault();

                                KotMain km = new KotMain();
                                km = kpO;
                                km.KotProdList = tempList;
                                con.Execute(InsertQueryForKotPrint, GetPrintKotForSave(km, user), tran);

                                con.Execute("UPDATE RMD_KOTMAIN_STATUS SET STATUS='CANCELED', REMARKS='" + cancelRemarks + "' WHERE KOTID=" + KOTID, transaction: tran);

                                tran.Commit();
                                return ("success");

                            }
                            catch (Exception ex) { tran.Rollback(); return ("Can't cancel the table..." + ex); }

                        }

                        else
                        {
                            return "User doesnot have access or PreBill status is running.";
                        }
                    }

                }
                
            }
            catch(Exception ex)
            {
                return ex.Message;                
            }
        }

        private static ObservableCollection<PrintKot> GetPrintKotForSave(KotMain kmO,string user)
        {
            ObservableCollection<PrintKot> pklist = new ObservableCollection<PrintKot>();
            foreach (var kp in kmO.KotProdList)
            {

                PrintKot pk = new PrintKot();
                pk.TABLENO = kp.TABLENO;
                pk.DESCA = kp.ItemDesc;
                pk.MENUCODE = getMenuCode(kp.MCODE);
                pk.ISBOT = kp.ISBOT;
                pk.QUANTITY = kp.Quantity;
                pk.REMARKS = kp.Remarks;
                pk.SNO = kp.SNO;
                pk.UNIT = kp.UNIT;
                pk.MCODE = kp.MCODE;
                pk.TRNDATE = kp.TRNDATE==null?DateTime.Today:kp.TRNDATE.Value;
                pk.REFSNO = kp.REFSNO;
                pk.KOTID = kmO.KOTID;
                //DateTimeFormatInfo dtfi = new DateTimeFormatInfo();
                //dtfi.ShortTimePattern = "hh:mm:ss tt";
                //dtfi.TimeSeparator = ":";
                //DateTime objDate = Convert.ToDateTime(kp.KOTTIME, dtfi);

                // pk.KOTTIME = objDate;
                pk.KOTTIME = GlobalClass.ServerDate;
                pk.KOT = kp.KOT;
                pk.USERNAME = user;
                //pk.ComboItem = kp.ComboItemDesc;
                //pk.PAX = kmO.Pax;
                pklist.Add(pk);
            }
            return pklist;
        }

        private static string getMenuCode(string mcode)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionDbInfo.ConnectionString))
                {
                    con.Open();
                    var result = con.ExecuteScalar("select menucode from menuitem where mcode='" + mcode + "'");
                    return result.ToString();
                }
            }
            catch (Exception ex) { return ""; }
        }

        public static FunctionResponse getTable()
        {
            try
            {
                List<TableDetail> PackedTableList = new List<TableDetail>();
                List<TableDetail> TableList = new List<TableDetail>();
                string saveSetting = "";

                using (SqlConnection cnMain = new SqlConnection(ConnectionDbInfo.ConnectionString))
                {
                    TableList = (cnMain.Query<TableDetail>("SELECT DISTINCT(TABLENO),LayoutName,ImageName FROM TABLELIST ORDER BY TABLENO").ToList());

                    var dtt = cnMain.ExecuteScalar<string>("SELECT USERMD FROM AND_SETTING", cnMain);
                    if (dtt != null)
                    {
                        saveSetting = dtt.ToString();
                    }
                    if (saveSetting == "1")
                    {
                        PackedTableList = cnMain.Query<TableDetail>("SELECT KM.TABLENO FROM RMD_KOTMAIN KM JOIN RMD_KOTMAIN_STATUS KMS ON KM.KOTID=KMS.KOTID WHERE KMS.STATUS='ACTIVE'", cnMain).ToList();
                    }
                    else if (saveSetting == "0")
                    {
                        PackedTableList = cnMain.Query<TableDetail>("SELECT TABLENO FROM KOTMAIN", cnMain).ToList();
                    }

                    if (PackedTableList.Count > 0)
                    {
                        foreach (var table in PackedTableList)
                        {
                            TableList.Find(x => x.TableNo == table.TableNo).IsPacked = true;
                        }
                    }

                    return new FunctionResponse() { status = "ok", result = TableList };
                }
            }
            catch (Exception e)
            {
                return new FunctionResponse() { status = "error", Message = e.Message };
            }
        }

        public static FunctionResponse getPackedTable()
        {
            List<TableDetail> PackTables = new List<TableDetail>();
            string saveSetting = "";

            using (SqlConnection cnMain = new SqlConnection(ConnectionDbInfo.ConnectionString))
            {
                cnMain.Open();
                try
                {
                    var dtt = cnMain.ExecuteScalar<string>("SELECT USERMD FROM AND_SETTING", cnMain);
                    if (dtt != null)
                    {
                        saveSetting = dtt.ToString();
                    }
                    if (saveSetting == "1")
                    {
                        PackTables = cnMain.Query<TableDetail>("SELECT KM.TABLENO FROM RMD_KOTMAIN KM JOIN RMD_KOTMAIN_STATUS KMS ON KM.KOTID=KMS.KOTID WHERE KMS.STATUS='ACTIVE'", cnMain).ToList();
                    }
                    else if (saveSetting == "0")
                    {
                        PackTables = cnMain.Query<TableDetail>("SELECT TABLENO FROM KOTMAIN", cnMain).ToList();
                    }
                    return new FunctionResponse() { status = "ok", result = PackTables };

                }
                catch (Exception e)
                {
                    return new FunctionResponse() { status = "error", Message = e.Message };
                }
            }
        }

        public static FunctionResponse getTableItemsDetail(string TABLENO)
        {
            byte saveSetting;
            //string KOTPROD;
            try
            {
                using (SqlConnection cnMain = new SqlConnection(ConnectionDbInfo.ConnectionString))
                {
                    saveSetting = cnMain.ExecuteScalar<byte>("SELECT USERMD FROM AND_SETTING");
                    string Q = "";
                    if (saveSetting == 1)
                    {
                        Q = string.Format
                                   (
                                       @"SELECT MCODE, ItemDesc DESCA, ISNULL(SUM(Quantity),0) QUANTITY, MAX(KOTTIME) KOTTIME, MAX(UNIT) UNIT,
                                        CASE WHEN K.Remarks='' THEN 'No Remarks' ELSE K.REMARKS END AS REMARKS, 
                                        CAST(ISBOT AS VARCHAR) IsBarItem, CAST(SNO AS VARCHAR) SNO,ISNULL(MAX(REFSNO),0) REFSNO, MAX(KOT) KOT FROM RMD_KOTPROD K JOIN RMD_KOTMAIN_STATUS KMS ON KMS.KOTID=K.KOTID WHERE KMS.STATUS='ACTIVE' AND K.TABLENO ='{0}'  
                                        GROUP BY MCODE, ItemDesc, K.Remarks, ISBOT, SNo", TABLENO
                                   );
                    }
                    else
                    {
                        Q = string.Format
                                   (
                                       @"SELECT MCODE, ItemDesc DESCA, ISNULL(SUM(Quantity),0) QUANTITY, MAX(KOTTIME) KOTTIME, MAX(UNIT) UNIT,
                                        CASE WHEN K.Remarks='' THEN 'No Remarks' ELSE K.REMARKS END AS REMARKS, 
                                        CAST(ISBOT AS VARCHAR) IsBarItem, CAST(SNO AS VARCHAR) SNO,ISNULL(MAX(REFSNO),0) REFSNO, MAX(KOT) KOT FROM RMD_KOTPROD K JOIN RMD_KOTMAIN_STATUS KMS ON KMS.KOTID=K.KOTID WHERE KMS.STATUS='ACTIVE' AND K.TABLENO ='{0}'  
                                        GROUP BY MCODE, ItemDesc, K.Remarks, ISBOT, SNo", TABLENO
                                   );
                    }
                    // KOTPROD = (saveSetting == 1) ? "RMD_KOTPROD" : "KOTPROD";

                    var pax = cnMain.ExecuteScalar<string>(string.Format(@"select pax from  rmd_kotmain k JOIN RMD_KOTMAIN_STATUS KMS ON KMS.KOTID=K.KOTID WHERE KMS.STATUS='ACTIVE' AND K.TABLENO ='{0}'",TABLENO));

                    var result = cnMain.Query<KOTProd>(Q).ToList();
                    result.ForEach(x => x.PAX = pax);

                    result.ForEach(x => x.ItemDesc = x.DESCA);
                    //if (result.Count > 0)
                    //{
                    //    result.ForEach(x => x.IsSaved = true);
                    //}

                    return new FunctionResponse() { status = "ok", result = result };
                }
            }
            catch (Exception e)
            {
                return new FunctionResponse() { status = "error", Message = e.Message };
            }

        }

        public static string postsaveOrders_New(KOTListTransfer KOTData)
        {
            try
            {
                string TABLENO, TRNUSER, PAX;

                List<KOTProd> ProdList = KOTData.KOTProdList;


                TABLENO = KOTData.TABLENO;
                TRNUSER = KOTData.TRNUSER;
                PAX = KOTData.PAX;

                byte vat = 0;
                double confactor = 0;
                double VATAMNT = 0.0, NETAMNT = 0.0, STAX = 0.0, TOTAMNT = 0.0;
                int KOTID;
                DateTime Now;
                //string printTofile = "";

                if (ProdList == null)
                {
                    return "Error: Null Items Send";
                }
                else if (ProdList.Count == 0)
                {
                    return "Error: No KOTProd Items";
                }


                using (SqlConnection cnMain = new SqlConnection(ConnectionDbInfo.ConnectionString))
                {
                    cnMain.Open();
                    try
                    {

                        Now = cnMain.ExecuteScalar<DateTime>("SELECT GETDATE()");
                        // if (cnMain.ExecuteScalar<byte>("SELECT ISNULL(MIN(P.STATUS), 128) FROM RMD_KOTMAIN K LEFT JOIN RMD_KOTMAIN_PREBILL P ON K.KOTID = P.KOTID AND K.TABLENO = P.TABLENO WHERE K.TABLENO = '" + TABLENO + "'") < 2)
                        if (cnMain.ExecuteScalar<byte>("SELECT ISNULL(MIN(P.STATUS), 128) FROM RMD_KOTMAIN K JOIN RMD_KOTMAIN_STATUS KMS ON K.KOTID=KMS.KOTID LEFT JOIN RMD_KOTMAIN_PREBILL P ON K.KOTID = P.KOTID AND K.TABLENO = P.TABLENO WHERE KMS.STATUS='ACTIVE' AND K.TABLENO = '" + TABLENO + "'") < 2)
                            return "Pre-Bill is pending for this table. Thus, you cannot add/remove order on this table.";
                        using (SqlTransaction trnOrder = cnMain.BeginTransaction())
                        {
                            //printTofile = TABLENO + "," + DateTime.Today + Environment.NewLine;
                            try
                            {

                                if (string.IsNullOrEmpty(cnMain.ExecuteScalar<string>("SELECT VNAME FROM RMD_SEQUENCES WHERE VNAME='KOTID'", transaction: trnOrder)))
                                {
                                    cnMain.Execute("INSERT INTO RMD_SEQUENCES (VNAME,AUTO,Start,CurNo,Division) VALUES('KOTID',1,1,1,'" + GlobalClass.DIVISION + "')", transaction: trnOrder);
                                }


                                foreach (KOTProd kp in ProdList)
                                {
                                    kp.TABLENO = TABLENO;
                                    var ItemInfo = cnMain.Query<dynamic>("SELECT RATE_A, VAT, DESCA, ISBARITEM FROM MENUITEM WHERE MCODE='" + kp.MCODE + "'", transaction: trnOrder).FirstOrDefault();
                                    //kp.RATE = Convert.ToDouble(ItemInfo.RATE_A);
                                    vat = ItemInfo.VAT;
                                    kp.ISBOT = ItemInfo.ISBARITEM;

                                    var AltItemInfo = cnMain.Query<dynamic>("SELECT ALTUNIT, CONFACTOR, RATE FROM MULTIALTUNIT WHERE MCODE='" + kp.MCODE + "' AND ALTUNIT='" + kp.UNIT + "'", transaction: trnOrder);
                                    if (AltItemInfo.Count() > 0)
                                    {
                                        kp.ALTUNIT = AltItemInfo.First().ALTUNIT;
                                        confactor = Convert.ToDouble(AltItemInfo.First().CONFACTOR);
                                        kp.RATE = Convert.ToDouble(AltItemInfo.First().RATE);
                                        kp.RealQty = kp.Quantity * confactor;
                                        kp.REALRATE = kp.RATE / confactor;
                                        kp.AltQty = kp.Quantity;
                                    }
                                    else
                                    {
                                        kp.RealQty = kp.Quantity;
                                        kp.REALRATE = kp.RATE;
                                    }
                                    kp.AMOUNT = kp.RATE * kp.Quantity;
                                    kp.SERVICETAX = kp.AMOUNT * 0.1;
                                    if (vat == 1)
                                    {
                                        kp.VAT = (kp.AMOUNT + kp.SERVICETAX) * 0.13;
                                    }
                                    kp.NAMNT = kp.AMOUNT + kp.SERVICETAX + kp.VAT;
                                    TOTAMNT += kp.AMOUNT;
                                    STAX += kp.SERVICETAX;
                                    VATAMNT += kp.VAT;
                                    NETAMNT += kp.NAMNT;
                                }
                                //int SNO = cnMain.ExecuteScalar<int>("SELECT ISNULL(MAX(SNO), 0) FROM RMD_KOTPROD WHERE TABLENO='" + TABLENO + "'", transaction: trnOrder);
                                int SNO = cnMain.ExecuteScalar<int>("SELECT ISNULL(MAX(SNO), 0) FROM RMD_KOTPROD KP JOIN RMD_KOTMAIN_STATUS KMS ON KP.KOTID=KMS.KOTID WHERE KMS.STATUS='ACTIVE' AND KP.TABLENO='" + TABLENO + "'", transaction: trnOrder);

                                if (SNO == 0)
                                {
                                    KOTID = cnMain.ExecuteScalar<int>("UPDATE RMD_SEQUENCES SET CurNo = CurNo + 1 output inserted.CurNo WHERE VNAME = 'KOTID'", transaction: trnOrder);
                                    cnMain.Execute(@"INSERT INTO RMD_KOTMAIN (TABLENO, TRNDATE, TRNUSER, DIVISION, TRNTIME, TERMINAL, WAITER, TOTAMNT, VATAMNT, NETAMNT, STAX, PAX, KOTID) SELECT '" + TABLENO
                                        + "','" + Now.ToString("MM/dd/yyyy") + "', '" + TRNUSER + "','" + ConnectionDbInfo.DIVISION + "','" + Now.ToString("h:mm:ss tt") + "','" + ConnectionDbInfo.TERMINAL + "','" + TRNUSER + "','" + TOTAMNT + "','" + VATAMNT + "','" + NETAMNT + "','" + STAX + "','" + PAX + "', '" + KOTID + "'", transaction: trnOrder);
                                    cnMain.Execute(QUERY_RMD_KOTMAIN_STATUS, new KOTMAINSTATUS { TABLENO = TABLENO, STATUS = "ACTIVE", KOTID = KOTID }, transaction: trnOrder);

                                }
                                else
                                {
                                    //  KOTID = cnMain.ExecuteScalar<int>("UPDATE RMD_KOTMAIN SET TRNDATE = '" + Now.ToString("MM/dd/yyyy") + "',TRNUSER='" + TRNUSER + "',TOTAMNT='" + TOTAMNT + "',VATAMNT='" + VATAMNT + "',NETAMNT='" + NETAMNT + "',STAX='" + STAX + "' OUTPUT INSERTED.KOTID WHERE TABLENO='" + TABLENO + "'", transaction: trnOrder);
                                    var alreadyOrderedKot = cnMain.Query("SELECT KP.* FROM RMD_KOTPROD KP JOIN RMD_KOTMAIN_STATUS KMS ON KMS.KOTID=KP.KOTID WHERE KMS.STATUS='ACTIVE' AND KP.TABLENO='" + TABLENO + "'", transaction: trnOrder);
                                    KOTID = cnMain.ExecuteScalar<int>("UPDATE RMD_SEQUENCES SET CurNo = CurNo + 1 output inserted.CurNo WHERE VNAME = 'KOTID'", transaction: trnOrder);
                                    int updatedStatusId = cnMain.ExecuteScalar<int>("UPDATE RMD_KOTMAIN_STATUS SET STATUS='UPDATE',REMARKS='" + "ID-" + KOTID + "' output inserted.KOTID WHERE STATUS='ACTIVE' AND TABLENO='" + TABLENO + "'", transaction: trnOrder);

                                    cnMain.Execute(@"INSERT INTO RMD_KOTMAIN (TABLENO, TRNDATE, TRNUSER, DIVISION, TRNTIME, TERMINAL, WAITER, TOTAMNT, VATAMNT, NETAMNT, STAX, PAX, KOTID) SELECT '" + TABLENO
                                        + "','" + Now.ToString("MM/dd/yyyy") + "', '" + TRNUSER + "','" + ConnectionDbInfo.DIVISION + "','" + Now.ToString("h:mm:ss tt") + "','" + ConnectionDbInfo.TERMINAL + "','" + TRNUSER + "','" + TOTAMNT + "','" + VATAMNT + "','" + NETAMNT + "','" + STAX + "','" + PAX + "', '" + KOTID + "'", transaction: trnOrder);
                                    cnMain.Execute(@"INSERT INTO RMD_KOTPROD (TABLENO, MCODE, UNIT, QUANTITY, REALQTY, AMOUNT, ITEMDESC, KOTTIME, KITCHENDISPATCH, Remarks, DIVISION, TRNDATE, RATE, REALRATE, VAT, SERVICETAX, AltQty, DISCOUNT, WAREHOUSE, NAMNT, ISBOT, KOT, ALTUNIT, WAITERNAME, SNO, REFSNO, KOTID) 
                                                    VALUES (@TABLENO, @MCODE, @UNIT, @QUANTITY, @REALQTY, @AMOUNT, @ITEMDESC, @KOTTIME, @KITCHENDISPATCH, @Remarks, @DIVISION, @TRNDATE, @RATE, @REALRATE, @VAT, @SERVICETAX, @AltQty, @DISCOUNT,@WAREHOUSE , @NAMNT, @ISBOT,@KOT, @ALTUNIT, @WAITERNAME, @SNO,@REFSNO, " + KOTID + ")", alreadyOrderedKot, transaction: trnOrder);
                                    cnMain.Execute(QUERY_RMD_KOTMAIN_STATUS, new KOTMAINSTATUS { TABLENO = TABLENO, STATUS = "ACTIVE", KOTID = KOTID }, transaction: trnOrder);

                                }



                                foreach (var kp in ProdList)
                                {
                                    //for new order entry
                                    if (kp.KOTTIME.Equals("not set"))
                                    {
                                        kp.SNO = ++SNO;
                                        kp.KOTTIME = Now.ToString("hh:mm:ss tt");
                                        kp.DIVISION = ConnectionDbInfo.DIVISION;
                                        kp.TRNDATE = Now.Date;
                                        kp.WAITERNAME = TRNUSER;
                                        if (kp.Remarks == null)
                                            kp.Remarks = "";
                                        
                                        cnMain.Execute(@"INSERT INTO RMD_KOTPROD (TABLENO, MCODE, UNIT, QUANTITY, REALQTY, AMOUNT, ITEMDESC, KOTTIME, KITCHENDISPATCH, Remarks, DIVISION, TRNDATE, RATE, REALRATE, VAT, SERVICETAX, AltQty, DISCOUNT, WAREHOUSE, NAMNT, ISBOT, KOT, ALTUNIT, WAITERNAME, SNO, REFSNO, KOTID) 
                                                    VALUES (@TABLENO, @MCODE, @UNIT, @QUANTITY, @REALQTY, @AMOUNT, @ITEMDESC, @KOTTIME, 0, @REMARKS, @DIVISION, @TRNDATE, @RATE, @REALRATE, @VAT, @SERVICETAX, @AltQty, 0, (SELECT NAME FROM RMD_WAREHOUSE WHERE ISDEFAULT='T'), @NAMNT, @ISBOT,(SELECT CurNo+1 FROM RMD_SEQUENCES WHERE VNAME='KOT'), @ALTUNIT, @WAITERNAME, @SNO, @REFSNO, " + KOTID + ")", kp, transaction: trnOrder);

                                        cnMain.Execute("INSERT INTO PRINTKOT (TABLENO, DESCA, MENUCODE, ISBOT, QUANTITY, REMARKS, SNO, UNIT, MCODE, TRNDATE, KOTTIME, USERNAME, KOT, PAX, KOTID) SELECT '" + TABLENO
                                            + "', DESCA, MENUCODE, ISBARITEM,'" + kp.Quantity + "','" + ((kp.Remarks.ToLower() == "no remarks") ? "" : kp.Remarks) + "','" + kp.SNO + "','" + kp.UNIT + "','" + kp.MCODE
                                                        + "','" + Now.ToString("MM/dd/yyyy") + "', '" + Now.ToString("h:mm:ss tt") + "','" + TRNUSER + "',(SELECT CurNo FROM RMD_SEQUENCES WHERE VNAME='KOT'), " + (string.IsNullOrEmpty(PAX) ? "0" : PAX) + ", " + KOTID + "  FROM MENUITEM WHERE MCODE='" + kp.MCODE + "'", transaction: trnOrder);
                                    }
                                }
                                cnMain.Execute("UPDATE RMD_SEQUENCES SET CURNO=CURNO + 1 WHERE VNAME ='KOT'", transaction: trnOrder);

                                trnOrder.Commit();
                                return "Success";
                            }
                            catch (Exception EX)
                            {
                                trnOrder.Rollback();
                                return EX.Message;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        return e.Message;
                    }
                }
            }
            catch
            {
                return "Error in Data Conversion";
            }
        }
    }
}
