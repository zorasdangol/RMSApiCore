using System;
using System.Collections.Generic;
using System.Text;

namespace KOTapiStandardLibrary.Models
{
    public class BillMain
    {
        public string BillNo { get; set; }
        public string Date { get; set; }
        public string BsDate { get; set; }
        public string VatNo { get; set; }
        public decimal GrossTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Taxable { get; set; }
        public decimal NonTaxable { get; set; }
        public decimal ServiceCharge { get; set; }
        public decimal Vat { get; set; }
        public decimal Net { get; set; }
        public List<BillProd> items { get; set; }
    }
}
