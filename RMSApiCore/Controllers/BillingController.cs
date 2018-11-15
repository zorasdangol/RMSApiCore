using Microsoft.AspNetCore.Mvc;
using KOTapiStandardLibrary.Business;
using KOTapiStandardLibrary.Models;
namespace RMSApiCore.Controllers
{

    public class BillingController : Controller
    {
        [Produces("application/json")]
        [Route("api/GenerateBill/{TableNo}")]
        public IActionResult GenerateBill(string TableNo)
        {
            BillGenerationMethod billGenerationMethod = new BillGenerationMethod();
            BillMain bill= billGenerationMethod.GenerateBill(TableNo);
            return new OkObjectResult(new { ok = "ok",obj=bill });
        }

        
    }
}