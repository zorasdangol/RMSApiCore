using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RMSApiCore
{
    public class NewHub: Hub
    {        
        public async Task Send(string name, string message)
        {
            // Call the broadcastMessage method to update clients.  
             await Clients.Others.SendAsync(name, message);
        }
    }
}
