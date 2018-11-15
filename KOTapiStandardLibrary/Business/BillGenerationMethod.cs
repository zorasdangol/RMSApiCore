using KOTapiStandardLibrary.Models;
using POSstandardLibrary.DataAccessLayer;
using ImsPosLibraryCore.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace KOTapiStandardLibrary.Business
{
    public class BillGenerationMethod
    {
        public BillMain GenerateBill(string TableNo)
        {
            KOTHelpler kothelper = new KOTHelpler();
            TrnMainBaseModel trnMain = kothelper.GenerateBill(TableNo);
            //TrnMain.ReCalculateBill();
            var bill = new BillMain();
            bill.GrossTotal = trnMain.TOTAMNT;
            bill.Discount = trnMain.DISVALUE;
            bill.Taxable = trnMain.TAXABLE;
            bill.NonTaxable = trnMain.NONTAXABLE;
            bill.ServiceCharge = trnMain.ServiceCharge;
            bill.Vat = trnMain.VATAMNT;
            bill.items = new List<BillProd>();
            var prods= trnMain.ProdList.Select(x => new BillProd() { Sno = x.SNO, Code = x.MCODE, Particular = x.ITEMDESC, Unit = x.UNIT, Quantity = x.Quantity, Rate = x.RATE, Amount = x.AMOUNT }).ToList();
            if (prods != null) bill.items = prods;

            return bill;
        }
        
    }
}
