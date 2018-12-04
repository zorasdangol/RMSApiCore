using Microsoft.AspNetCore.Mvc;
using KOTapiStandardLibrary.Business;
using KOTAppClassLibrary.Models;

namespace RMSApiCore.Controllers
{

    public class BillingController : Controller
    {
        [Produces("application/json")]
        [Route("api/GenerateBill/{TableNo}")]
        public FunctionResponse GenerateBill(string TableNo)
        {
            BillGenerationMethod billGenerationMethod = new BillGenerationMethod();
            return billGenerationMethod.GenerateBill(TableNo);
        }

        
    }
}