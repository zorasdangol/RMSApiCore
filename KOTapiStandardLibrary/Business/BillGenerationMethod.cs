
using POSstandardLibrary.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using KOTAppClassLibrary.Models;

namespace KOTapiStandardLibrary.Business
{
    public class BillGenerationMethod
    {
        public KOTAppClassLibrary.Models.FunctionResponse GenerateBill(string TableNo)
        {
            try
            {
                KOTHelpler kothelper = new KOTHelpler();
                POSstandardLibrary.Models.TrnMainBaseModel trnMain = kothelper.GenerateBill(TableNo);
                //TrnMain.ReCalculateBill();
                var bill = new BillMain();
                bill.GrossTotal = trnMain.TOTAMNT;
                bill.Discount = trnMain.DISVALUE;
                bill.Taxable = trnMain.TAXABLE;
                bill.NonTaxable = trnMain.NONTAXABLE;
                bill.ServiceCharge = trnMain.ServiceCharge;
                bill.Vat = trnMain.VATAMNT;
                bill.items = new List<BillProd>();
                var prods = trnMain.ProdList.Select(x => new BillProd() { Sno = x.SNO, Code = x.MCODE, Particular = x.ITEMDESC, Unit = x.UNIT, Quantity = x.Quantity, Rate = x.RATE, Amount = x.AMOUNT }).ToList();
                if (prods != null) bill.items = new List<BillProd>(prods);

                return new KOTAppClassLibrary.Models.FunctionResponse() { status = "ok", result = bill };
            }
            catch(Exception ex)
            {
                return new KOTAppClassLibrary.Models.FunctionResponse() { status = "error", Message = ex.Message };
            }
        }
        
    }
}
