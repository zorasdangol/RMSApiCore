using Dapper;
using KOTAppClassLibrary.Models;
using POSstandardLibrary.Helper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace KOTapiStandardLibrary.Business
{
    public class TableDispatchMethods
    {
        public FunctionResponse getAllKOTProd(string user)
        {

            try
            {
                FunctionResponse functionResponse = new FunctionResponse();

                using (SqlConnection cnMain = new SqlConnection(ConnectionDbInfo.ConnectionString))
                {
                    var items = cnMain.Query<KOTProd>("SELECT KP.* FROM RMD_KOTPROD KP JOIN RMD_KOTMAIN_STATUS KMS ON KMS.KOTID=KP.KOTID WHERE KMS.STATUS='ACTIVE'");
                    if (items == null)
                    {
                        functionResponse.status = "error";
                        functionResponse.Message = "No Items found to serve.";
                    }
                    else
                    {
                        functionResponse.status = "ok";
                        functionResponse.result = items;
                    }

                    return functionResponse;
                }

            }
            catch (Exception ex)
            {
                return new FunctionResponse() { status = "error", Message = ex.Message };
            }
        }

        public string SaveKitchenDispatch(KOTProd order)
        {
            try
            {
                using (SqlConnection cnMain = new SqlConnection(ConnectionDbInfo.ConnectionString))
                {
                    var items = cnMain.Execute("update rmd_kotprod set  Kitchendispatch = 1, dispatchUser='" + order.DispatchUser + "', dispatchtime = '" + DateTime.Now + "', Remarks = '" + order.Remarks + "' where mcode='" + order.MCODE + "' and kotid=" + order.KOTID + " and tableno='" + order.TABLENO + "' and sno=" + order.SNO + " and KOT=" + order.KOT);

                    if (items == 0)
                    {
                        return "Server error: No item dispatch";
                    }
                    else
                    {
                        items = cnMain.Execute("update rmd_kotprod set  Kitchendispatch = 1, dispatchUser='" + order.DispatchUser + "', dispatchtime = '" + DateTime.Now + "', Remarks = '" + order.Remarks + "' where mcode='" + order.MCODE + "' and kotid=" + order.KOTID + " and tableno='" + order.TABLENO + "' and RefSNO=" + order.SNO);

                        return "success";
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
