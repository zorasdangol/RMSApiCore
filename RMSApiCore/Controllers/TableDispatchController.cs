using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KOTapiStandardLibrary.Business;
using KOTAppClassLibrary.Models;
using Microsoft.AspNetCore.Mvc;

namespace RMSApiCore.Controllers
{
    public class TableDispatchController : Controller
    {
        [Route("api/getAllKOTProd")]
        [HttpGet("{user}")]
        public FunctionResponse getAllKOTProd(string user)
        {
            return new TableDispatchMethods().getAllKOTProd(user);
        }        

        [Route("api/SaveKitchenDispatch")]
        [HttpPost]
        public string SaveKitechDispatch([FromBody]KOTProd Order)
        {
            return new TableDispatchMethods().SaveKitchenDispatch(Order);
        }
    }
}