using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using bi130168d.Controllers;

namespace bi130168d.Hubs
{
    public class MyHub:Hub
    {
        private static AuctionsController ac = new AuctionsController();

        public void Bid(string username, int id)
        {
            string result = ac.Bid(id, username);
            if (result == "OK")
            {
                Clients.All.newBid(username, id);
            }
            else
            {
                Clients.All.failed(result, username);
            }
        }
    }
}
