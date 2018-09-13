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
    public class GetTableDetailsController : Controller
    {
        [Route("api/GetDayCloseTable")]
        [HttpGet("{dayCloseTable}")]
        public String getdayCloseTable()
        {
            return TableDetailsMethods.getdayCloseTable();

        }

        [Route("api/GetTable")]
        [HttpGet("{GetTable}")]
        public FunctionResponse getTable()
        {
            return TableDetailsMethods.getTable();
        }

        [Route("api/GetPackedTable")]
        [HttpGet("{getPackedTable}")]
        public FunctionResponse getPackedTable()
        {
            return TableDetailsMethods.getPackedTable();
        }

        [Route("api/GetTableItemsDetail")]
        [HttpGet("{TABLENO}")]
        public FunctionResponse getTableItemsDetail(string TABLENO)
        {
            return TableDetailsMethods.getTableItemsDetail(TABLENO);
        }


        [Route("api/GetTableDetails")]
        [HttpPost]
        public string postsaveOrders_New([FromBody]KOTListTransfer KOTData)
        {
            return TableDetailsMethods.postsaveOrders_New(KOTData);
        }
    }
}