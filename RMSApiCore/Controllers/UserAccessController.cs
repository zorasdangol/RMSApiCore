using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
            return new UserAccessMethods().postuserVerification(User);
        }
        
        [Route("api/CheckAccess")]
        [HttpPost]
        public string CheckAccess([FromBody] User User)
        {
            return new UserAccessMethods().CheckAccess(User);            
        }
    }
}