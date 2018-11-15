using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImsPosLibraryCore.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RMSApiCore.Controllers
{
 
    public class DefaultController : Controller
    {
        [Produces("application/json")]
        [Route("api/GetConnectionString")]
        public string GetConnectionString()
        {
            return ConnectionDbInfo.ConnectionString;
        }

        [Produces("application/json")]
        [Route("api/GetApplicationPath")]
        public string GetApplicationPath()
        {
            return ConnectionDbInfo.AppDataPath;
        }


        [Produces("application/json")]
        [Route("api/GetConnectionList")]
        public List<ConnectionModel> GetConnectionList()
        {
            return ConnectionDbInfo.ConnectionList;
        }
    }
}