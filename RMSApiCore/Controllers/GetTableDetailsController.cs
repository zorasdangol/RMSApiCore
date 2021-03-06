﻿using System;
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
            return new TableDetailsMethods().getdayCloseTable();

        }

        [Route("api/GetTable")]
        [HttpGet("{GetTable}")]
        public FunctionResponse getTable()
        {
            return new TableDetailsMethods().getTable();
        }

        [Route("api/GetPackedTable")]
        [HttpGet("{getPackedTable}")]
        public FunctionResponse getPackedTable()
        {
            return new TableDetailsMethods().getPackedTable();
        }

        [Route("api/GetTableItemsDetail")]
        [HttpGet("{TABLENO}")]
        public FunctionResponse getTableItemsDetail(string TABLENO)
        {
            return new TableDetailsMethods().getTableItemsDetail(TABLENO);
        }


        [Route("api/GetTableDetails")]
        [HttpPost]
        public string postsaveOrders_New([FromBody]KOTListTransfer KOTData)
        {
            return new TableDetailsMethods().postsaveOrders_New(KOTData);
        }


        [Route("api/CancelOrders/{tableNo}/{user}/{remarks}")]
        //[HttpGet("CancelOrders/{tableNo}/{user}/{remarks}")]
        public string cancelOrders(string tableNo,string user, string remarks)
        {
            return new TableDetailsMethods().CancelOrder(tableNo, user, remarks);
            //return TableDetailsMethods.getTableItemsDetail(TABLENO);
            return null;
        }

    }
}