using System;
using System.Collections.Generic;
using System.Text;

namespace KOTapiStandardLibrary.Models
{
    public class BillProd
    {
        public int Sno { get; set; }
        public string Code { get; set; }
        public string Particular { get; set; }
        public string Unit { get; set; }
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
    }
}
