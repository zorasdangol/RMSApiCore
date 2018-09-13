using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KOTapiStandardLibrary.Business;
//using KOTapiStandardLibrary.Business;
using KOTAppClassLibrary.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RMSApiCore.Controllers
{
    [Produces("application/json")]
    [Route("api/GetMenuItems")]
    public class GetMenuItemsController : Controller
    {
        [HttpGet]
        public FunctionResponse getMenuItem_New()
        {
            return MenuItemsMethods.MenuItem_New();

        }
    }
}