using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KOTapiStandardLibrary.Business;
using KOTAppClassLibrary.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RMSApiCore.Controllers
{
    public class UserAccessController : Controller
    {

        [Route("api/UserVerification")]
        [HttpPost]
        public string postuserVerification([FromBody]User User)
        {
            return UserAccessMethods.postuserVerification(User);
        }
    }
}