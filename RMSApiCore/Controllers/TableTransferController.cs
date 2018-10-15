﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using KOTapiStandardLibrary.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using KOTAppClassLibrary.Models;
using Newtonsoft.Json;
using KOTapiStandardLibrary.Business;

namespace RMSApiCore.Controllers
{
    
    public class TableTransferController : Controller
    {
        [Route("api/transferAllTable")]
        [HttpGet("{tableNew}/{tableOld}")]
        public string transferAllTable(string tableNew, string tableOld)
        {
            return TableTransferMethods.transferAllTable(tableNew, tableOld);
        }


        [Route("api/MergeTable")]
        [HttpPost]
        public string MergeTable([FromBody] MergeTransfer mergeTransfer)
        {
            return TableTransferMethods.MergeTable(mergeTransfer);           
        }

        [Route("api/SplitTable")]
        [HttpPost]   
        public string SplitTable([FromBody] SplitTransfer SplitTransfer)
        {
            return TableTransferMethods.SplitTable(SplitTransfer);           

        }


    }
}