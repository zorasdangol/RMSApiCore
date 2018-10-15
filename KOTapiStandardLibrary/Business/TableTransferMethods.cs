using KOTapiStandardLibrary.Helpers;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using System.Text;
using KOTAppClassLibrary.Models;
using Newtonsoft.Json;

namespace KOTapiStandardLibrary.Business
{
    public static class TableTransferMethods
    {
        public static string transferAllTable(string tableNew, string tableOld)
        {
            string saveSetting = "";
            string count = "";

            using (SqlConnection cnMain = new SqlConnection(ConnectionDbInfo.ConnectionString))
            {
                cnMain.Open();
                try
                {


                    saveSetting = cnMain.ExecuteScalar<string>("SELECT USERMD FROM AND_SETTING", cnMain);

                    if (saveSetting == "1")
                    {
                        count = cnMain.ExecuteScalar<string>("SELECT COUNT(*) FROM RMD_KOTMAIN KM JOIN RMD_KOTMAIN_STATUS KMS ON KM.KOTID=KMS.KOTID WHERE KMS.STATUS='ACTIVE' AND KM.TABLENO='" + tableNew + "'", cnMain);

                        if (count == "0")
                        {
                            var oldkotMAIN = cnMain.Query<KotMain>("SELECT KM.* FROM RMD_KOTMAIN KM JOIN RMD_KOTMAIN_STATUS KMS ON KMS.KOTID=KM.KOTID WHERE KMS.STATUS='ACTIVE' AND KM.TABLENO='" + tableOld + "'").FirstOrDefault();
                            var oldkotPROD = cnMain.Query<KOTProd>("SELECT KP.* FROM RMD_KOTPROD KP JOIN RMD_KOTMAIN_STATUS KMS ON KMS.KOTID=KP.KOTID WHERE KMS.STATUS='ACTIVE' AND KP.TABLENO='" + tableOld + "'").ToList();
                            int oldKOTID = cnMain.ExecuteScalar<int>("UPDATE RMD_KOTMAIN_STATUS SET STATUS='TRANSFER' output inserted.KOTID WHERE STATUS='ACTIVE' AND TABLENO='" + tableOld + "'");
                            int KOTID = cnMain.ExecuteScalar<int>("UPDATE RMD_SEQUENCES SET CurNo = CurNo + 1 output inserted.CurNo WHERE VNAME = 'KOTID'");

                            oldkotMAIN.TABLENO = tableNew;
                            oldkotMAIN.KOTID = KOTID;
                            oldkotPROD.ForEach(x => x.TABLENO = tableNew);
                            oldkotPROD.ForEach(x => x.KOTID = KOTID);
                            cnMain.Execute("INSERT INTO RMD_KOTMAIN_STATUS (KOTID,TABLENO,STATUS) VALUES(@KOTID,@TABLENO,@STATUS)", new KOTMAINSTATUS { KOTID = KOTID, TABLENO = tableNew, STATUS = "ACTIVE" });
                            cnMain.Execute(@"INSERT INTO RMD_KOTMAIN (TABLENO, TRNDATE,TRNTIME,TOTAMNT,DCAMNT,DCRATE,VATAMNT,NETAMNT,WAITER,TRNUSER,INDDIS,REMARKS,TERMINAL,EDITUSER,BILLNO,DCRATE2,DIVISION,STAX,MEMID,BILLTO,BILLTOVAT,FLG,BILLTOADD,ROUNDUP,PAX,BILLED,STATUS,REFBILLED,PHISCALID, KOTID) 
                                        VALUES(@TABLENO, @TRNDATE,@TRNTIME,@TOTAMNT,@DCAMNT,@DCRATE,@VATAMNT,@NETAMNT,@WAITER,@TRNUSER,@INDDIS,@REMARKS,@TERMINAL,@EDITUSER,@BILLNO,@DCRATE2,@DIVISION,@STAX,@MEMID,@BILLTO,@BILLTOVAT,@FLG,@BILLTOADD,@ROUNDUP,@PAX,@BILLED,@STATUS,@REFBILLED,@PHISCALID,@KOTID)", oldkotMAIN);
                            cnMain.Execute(@"INSERT INTO RMD_KOTPROD(KOTID,TABLENO,TRNDATE,MCODE,UNIT,Quantity,RealQty,AltQty,RATE,AMOUNT,DISCOUNT,VAT,REALRATE,IDIS,ALTUNIT,WAREHOUSE,KOT,SNO,ItemDesc,DIVISION,SERVICETAX,NAMNT,KOTTIME,KitchenDispatch,DispatchUser,Remarks,ISBOT,ComboItem,ComboItemQty, WaiterName, REFSNO,RECEIPES,TAKEAWAY) VALUES(@KOTID,@TABLENO,@TRNDATE,@MCODE,@UNIT,@Quantity,@RealQty,@AltQty,@RATE,@AMOUNT,@DISCOUNT,@VAT,@REALRATE,@IDIS,@ALTUNIT,@WAREHOUSE,@KOT,@SNO,@ItemDesc,@DIVISION,@SERVICETAX,@NAMNT,@KOTTIME,@KitchenDispatch,@DispatchUser,@Remarks,@ISBOT,@ComboItem,@ComboItemQty, @WaiterName, @REFSNO,@RECEIPES,@TAKEAWAY)", oldkotPROD);
                            cnMain.ExecuteScalar<int>("UPDATE RMD_KOTMAIN_STATUS SET REMARKS='Transfer To Id- " + KOTID + "' WHERE KOTID='" + oldKOTID + "'");
                            // db.executeNonQuery("UPDATE RMD_KOTMAIN SET TABLENO='" + tableNew + "' WHERE TABLENO='" + tableOld + "'", trn, cnMain);
                            // db.executeNonQuery("UPDATE RMD_KOTPROD SET TABLENO='" + tableNew + "' WHERE TABLENO='" + tableOld + "'", trn, cnMain);

                            // db.executeNonQuery("DELETE FROM RMD_KOTMAIN WHERE TABLENO='" + tableOld + "'", trn, cnMain);

                            //pdt = db.getData("SELECT KOTFolder FROM SETTING", cnMain, trn);
                            //string destination = pdt.Rows[0]["KOTFolder"].ToString();

                            /*if (File.Exists(destination + "\\" + tableOld + ".ord"))
                            {
                                File.Delete(destination + "\\" + tableOld + ".ord");
                            }*/
                        }
                        else
                        {
                            return "no";
                        }

                    }
                    if (saveSetting == "0")
                    {
                        count = cnMain.ExecuteScalar<string>("SELECT COUNT(*) FROM KOTMAIN WHERE TABLENO='" + tableNew + "'", cnMain);

                        if (count == "0")
                        {
                            cnMain.Execute("UPDATE KOTMAIN SET TABLENO='" + tableNew + "' WHERE TABLENO='" + tableOld + "'", cnMain);
                            cnMain.Execute("UPDATE KOTPROD SET TABLENO='" + tableNew + "' WHERE TABLENO='" + tableOld + "'", cnMain);

                            cnMain.Execute("DELETE FROM KOTMAIN WHERE TABLENO='" + tableOld + "'", cnMain);
                        }
                        else
                        {
                            return "no";
                        }
                    }
                    return "success";
                }
                catch (Exception e)
                {
                    return e.Message;
                }
            }
            
        }

        public static string SplitTable(SplitTransfer SplitTransfer)
        {
            try
            {

                if (SplitTransfer == null)
                    return "No data Received";
                List<KOTProd> ProdList = SplitTransfer.transferData;
                var TABLENO = SplitTransfer.TableNo;
                var TRNUSER = SplitTransfer.TRNUSER;
                string WareHouse, PAX;
                double VATAMNT = 0.0, NETAMNT = 0.0, STAX = 0.0, TOTAMNT = 0.0;

                using (SqlConnection cnMain = new SqlConnection(ConnectionDbInfo.ConnectionString))
                {
                    cnMain.Open();
                    if (cnMain.ExecuteScalar<byte>("SELECT ISNULL(MIN(P.STATUS), 128) FROM RMD_KOTMAIN K JOIN RMD_KOTMAIN_STATUS KMS ON K.KOTID=KMS.KOTID LEFT JOIN RMD_KOTMAIN_PREBILL P ON K.KOTID = P.KOTID AND K.TABLENO = P.TABLENO WHERE KMS.STATUS='ACTIVE' AND K.TABLENO = '" + TABLENO + "'") < 2)
                        return "Pre-Bill is pending for this table. Thus, you cannot Split/Transfer order on this table.";
                    WareHouse = cnMain.ExecuteScalar<string>("SELECT NAME FROM RMD_WAREHOUSE WHERE ISDEFAULT='T'");
                    IEnumerable<dynamic> KPList = cnMain.Query<dynamic>("SELECT MCODE, KOT, KOTTIME, WaiterName, Quantity, RealQty, KP.KOTID FROM RMD_KOTPROD KP JOIN RMD_KOTMAIN_STATUS KMS ON KP.KOTID=KMS.KOTID WHERE KMS.STATUS='ACTIVE' AND KP.TABLENO = '" + TABLENO + "'");
                    if (KPList == null)
                    {
                        return "Data MisMatch Error: No Items found for this table";
                    }
                    int oldKotId = KPList.FirstOrDefault().KOTID;
                    using (SqlTransaction tran = cnMain.BeginTransaction())
                    {
                        // cnMain.Execute("DELETE FROM RMD_KOTMAIN WHERE TABLENO='" + TABLENO + "'", transaction: tran);
                        int updatedKotId = cnMain.ExecuteScalar<int>("UPDATE RMD_KOTMAIN_STATUS SET STATUS='SPLIT' output inserted.KOTID  WHERE STATUS='ACTIVE' AND TABLENO='" + TABLENO + "'", transaction: tran);
                        string RemarkStatus = "Split-";
                        foreach (string TBLNO in ProdList.Select(x => x.TABLENO).Distinct())
                        {
                            foreach (KOTProd kp in ProdList.Where(x => x.TABLENO == TBLNO))
                            {
                                foreach (dynamic KProd in KPList.Where(x => x.MCODE == kp.MCODE))
                                {
                                    bool Break = false;
                                    if (KProd.Quantity >= (decimal)kp.Quantity)
                                    {
                                        KProd.Quantity -= (decimal)kp.Quantity;
                                        Break = true;
                                    }
                                    else if (KProd.Quantity > 0)
                                    {
                                        kp.Quantity = (double)KProd.Quantity;
                                        KProd.Quantity -= (decimal)kp.Quantity;
                                    }
                                    else
                                        continue;
                                    kp.KOT = KProd.KOT;
                                    kp.WAITERNAME = KProd.WaiterName;
                                    kp.WAREHOUSE = WareHouse;
                                    var MenuItem = cnMain.Query("SELECT RATE_A, VAT, DESCA, BASEUNIT, ISBARITEM FROM MENUITEM WHERE MCODE='" + kp.MCODE + "'", transaction: tran).FirstOrDefault();
                                    kp.ItemDesc = MenuItem.DESCA;
                                    kp.UNIT = MenuItem.BASEUNIT;
                                    kp.ISBOT = MenuItem.ISBARITEM;
                                    kp.RATE = Convert.ToDouble(MenuItem.RATE_A);
                                    kp.AMOUNT = kp.RATE * kp.Quantity;
                                    kp.SERVICETAX = kp.AMOUNT * 0.1;
                                    if (MenuItem.VAT == 1)
                                    {
                                        kp.VAT = (kp.AMOUNT + kp.SERVICETAX) * 0.13;
                                    }
                                    kp.NAMNT = kp.AMOUNT + kp.SERVICETAX + kp.VAT;
                                    TOTAMNT += kp.AMOUNT;
                                    STAX += kp.SERVICETAX;
                                    VATAMNT += kp.VAT;
                                    NETAMNT += kp.NAMNT;
                                    if (Break)
                                        break;
                                }
                            }
                            PAX = ProdList.First(x => x.TABLENO == TBLNO).PAX;
                            int KOTID = cnMain.ExecuteScalar<int>("UPDATE RMD_SEQUENCES SET CurNo = CurNo + 1 output inserted.CurNo WHERE VNAME = 'KOTID'", transaction: tran);
                            string Remarks = "Split, " + TABLENO + ", " + oldKotId;
                            RemarkStatus += KOTID + ",";
                            cnMain.Execute("INSERT INTO RMD_KOTMAIN_STATUS (KOTID,TABLENO,STATUS) VALUES(@KOTID,@TABLENO,@STATUS)", new KOTMAINSTATUS { KOTID = KOTID, TABLENO = TBLNO, STATUS = "ACTIVE" }, transaction: tran);
                            cnMain.Execute(@"INSERT INTO RMD_KOTMAIN (TABLENO, TRNDATE, TRNUSER, DIVISION, TRNTIME, TERMINAL, WAITER, TOTAMNT, VATAMNT, NETAMNT, STAX, PAX, KOTID, EDITUSER) 
                                            VALUES('" + TBLNO + "',CONVERT(VARCHAR,GETDATE(),101), '" + TRNUSER + "','" + GlobalClass.Division + "','" + DateTime.Now.ToString("hh:mm:ss tt") + "','" + GlobalClass.Terminal + "','" + TRNUSER + "'," + TOTAMNT + "," + VATAMNT + "," + NETAMNT + "," + STAX + "," + PAX + ", " + KOTID + ", '" + Remarks + "')", transaction: tran);
                            foreach (KOTProd kp in ProdList.Where(x => x.TABLENO == TBLNO))
                            {
                                cnMain.Execute(@"INSERT INTO RMD_KOTPROD (TABLENO, MCODE, UNIT, QUANTITY, REALQTY, AMOUNT, ItemDesc, KOTTIME, KITCHENDISPATCH, Remarks, DIVISION, TRNDATE, RATE, REALRATE, VAT, SERVICETAX, AltQty, DISCOUNT, WAREHOUSE, NAMNT, ISBOT, KOT, SNO, WAITERNAME, KOTID) 
                                             VALUES (@TABLENO, @MCODE, @UNIT, @QUANTITY, @QUANTITY, @AMOUNT, @ItemDesc, @KOTTIME, 0, @Remarks, '" + GlobalClass.Division + "', CONVERT(VARCHAR,GETDATE(),101), @RATE, @RATE, @VAT, @SERVICETAX, 0, 0, @WAREHOUSE, @NAMNT, @ISBOT, @KOT, @SNO, @WAITERNAME, " + KOTID + ")", kp, transaction: tran);
                            }
                        }

                        cnMain.Execute("UPDATE RMD_KOTMAIN_STATUS SET REMARKS='" + RemarkStatus + "'  WHERE KOTID=" + updatedKotId, transaction: tran);
                        tran.Commit();
                        return "success";
                    }
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        //private void ExecutemergeTableCommand(object obj)
        //{
        //    try
        //    {
        //        string InsertQueryForKotProd = "INSERT INTO RMD_KOTPROD(KOTID,TABLENO,TRNDATE,MCODE,UNIT,Quantity,RealQty,AltQty,RATE,AMOUNT,DISCOUNT,VAT,REALRATE,IDIS,ALTUNIT,WAREHOUSE,KOT,SNO,ItemDesc,DIVISION,SERVICETAX,NAMNT,KOTTIME,KitchenDispatch,DispatchTime,DispatchUser,Remarks,ISBOT,ComboItem,ComboItemQty, WaiterName, REFSNO,RECEIPES,TAKEAWAY) VALUES(@KOTID,@TABLENO,@TRNDATE,@MCODE,@UNIT,@Quantity,@RealQty,@AltQty,@RATE,@AMOUNT,@DISCOUNT,@VAT,@REALRATE,@IDIS,@ALTUNIT,@WAREHOUSE,@KOT,@SNO,@ItemDesc,@DIVISION,@SERVICETAX,@NAMNT,@KOTTIME,@KitchenDispatch,@DispatchTime,@DispatchUser,@Remarks,@ISBOT,@ComboItem,@ComboItemQty, @WaiterName, @REFSNO,@RECEIPES,@TAKEAWAY)";
        //        var list = (popup.DataContext as MergeTableViewModel).TableListForMerge;
        //                List<KOTProd> kpMergedListForSave = new List<KOTProd>();

        //                using (SqlConnection con = new SqlConnection(ConnectionDbInfo.ConnectionString))
        //                {
        //                    con.Open();
        //                    SqlTransaction tran = con.BeginTransaction();
        //                    try
        //                    {
        //                        foreach (var o in list.Where(x => x.IsTableChecked))
        //                        {
        //                            var kotprodLis = new List<KOTProd>(con.Query<KOTProd>("select kp.* from rmd_kotprod kp join rmd_kotmain_status kms on kp.kotid=kms.kotid where kms.status='ACTIVE' AND kp.TableNo='" + o.TableNo + "'", null, tran));

        //                            if (kotprodLis.Count() == 0) { tran.Rollback(); MessageBox.Show("Error on merging Table.Sorry could not find KOT Items"); return; }
        //                            kotprodLis.ForEach(x => x.Remarks = "MERGE FROM KOTID:" + x.KOTID);
        //                            kpMergedListForSave.AddRange(kotprodLis);

        //                            con.Execute("UPDATE RMD_KOTMAIN_STATUS SET STATUS='MERGE',REMARKS='MERGE TO: " + KotMainObj.KOTID + "'  WHERE KOTID=" + kotprodLis.FirstOrDefault().KOTID, transaction: tran);
        //                        }

        //                        foreach (var t in kpMergedListForSave)
        //                        {
        //                            t.TABLENO = TableId;
        //                            t.KOTID = KotMainObj.KOTID;
        //                        }
        //                        con.Execute(InsertQueryForKotProd, kpMergedListForSave, tran);

        //                        tran.Commit(); //MessageBox.Show("Table Merging Successfull...");
        //                       // DialogResult = true;


        //                    }
        //                    catch (Exception ex) { 
        //                        //tran.Rollback(); MessageBox.Show("Error on merging table..." + ex); 
        //                    }
        //                }

        //            }
        //        }
        //    }
        //    catch (Exception ex) { MessageBox.Show("Error Message..." + ex); }

        //}

        public static string MergeTable(MergeTransfer mergeTransfer)
        {

            try
            {
                string DestinationTableNo = mergeTransfer.DestinationTableNo;                
            
                var list = mergeTransfer.MergingTables;

                string InsertQueryForKotProd = "INSERT INTO RMD_KOTPROD(KOTID,TABLENO,TRNDATE,MCODE,UNIT,Quantity,RealQty,AltQty,RATE,AMOUNT,DISCOUNT,VAT,REALRATE,IDIS,ALTUNIT,WAREHOUSE,KOT,SNO,ItemDesc,DIVISION,SERVICETAX,NAMNT,KOTTIME,KitchenDispatch,DispatchTime,DispatchUser,Remarks,ISBOT,ComboItem,ComboItemQty, WaiterName, REFSNO,RECEIPES,TAKEAWAY) VALUES(@KOTID,@TABLENO,@TRNDATE,@MCODE,@UNIT,@Quantity,@RealQty,@AltQty,@RATE,@AMOUNT,@DISCOUNT,@VAT,@REALRATE,@IDIS,@ALTUNIT,@WAREHOUSE,@KOT,@SNO,@ItemDesc,@DIVISION,@SERVICETAX,@NAMNT,@KOTTIME,@KitchenDispatch,@DispatchTime,@DispatchUser,@Remarks,@ISBOT,@ComboItem,@ComboItemQty, @WaiterName, @REFSNO,@RECEIPES,@TAKEAWAY)";
                
                List<KOTProd> kpMergedListForSave = new List<KOTProd>();
                KotMain KotMainObj = new KotMain();
                using (SqlConnection con = new SqlConnection(ConnectionDbInfo.ConnectionString))
                {
                    con.Open();
                    SqlTransaction tran = con.BeginTransaction();
                    try
                    {
                        KotMainObj = con.Query<KotMain>("Select * from RMD_KOTMAIN kp JOIN RMD_KOTMAIN_STATUS KMS ON KP.KOTID=KMS.KOTID  where KMS.STATUS='ACTIVE' and kp.TableNo='" + DestinationTableNo + "'", transaction:tran).FirstOrDefault();
                        foreach (var o in list)
                        {
                            var kotprodLis = new List<KOTProd>(con.Query<KOTProd>("select kp.* from rmd_kotprod kp join rmd_kotmain_status kms on kp.kotid=kms.kotid where kms.status='ACTIVE' AND kp.TableNo='" + o + "'",transaction:tran));

                            if (kotprodLis.Count() == 0) { tran.Rollback(); return ("Error on merging Table.Sorry could not find KOT Items"); }
                            kotprodLis.ForEach(x => x.Remarks = "MERGE FROM KOTID:" + x.KOTID);
                            kpMergedListForSave.AddRange(kotprodLis);

                            con.Execute("UPDATE RMD_KOTMAIN_STATUS SET STATUS='MERGE',REMARKS='MERGE TO: " + KotMainObj.KOTID + "'  WHERE KOTID=" + kotprodLis.FirstOrDefault().KOTID,transaction:tran);
                        }

                        foreach (var t in kpMergedListForSave)
                        {
                            t.TABLENO = DestinationTableNo;
                            t.KOTID = KotMainObj.KOTID;                            
                        }
                        con.Execute(InsertQueryForKotProd, kpMergedListForSave, transaction:tran);

                        tran.Commit(); 
                        return ("success");
                    }
                    catch (Exception ex)
                    {
                        return ("Error on merging table..." + ex.Message); 
                    }
                }
                
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


    }
}
