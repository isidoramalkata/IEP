using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using bi130168d.Models;
using PagedList;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNet.SignalR;
using System.Data.Entity.Infrastructure;
using bi130168d.Hubs;
using System.Data.Entity.Core;
using System.Data.SqlClient;

namespace bi130168d.Controllers
{
    public class AuctionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private Model1 dbUser = new Model1();

        // GET: Auctions
        public ActionResult Index(string currentFilter, string currentFilterMin, string currentFilterMax, string currentFilterStatus, string searchString, string min, string max, string status, int? page)
        {
            //checkState();
            var auctions = from a in db.Auctions
                           where a.Active == true
                           && a.State != AuctionState.Draft
                           select a;

            if (searchString != null || min != null || max != null || status != null)
                page = 1;
            else
            {
                searchString = currentFilter;
                min = currentFilterMin;
                max = currentFilterMax;
                status = currentFilterStatus;
            }


            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentFilterMin = min;
            ViewBag.CurrentFilterMax = max;
            ViewBag.currentFilterStatus = status;

            if (!String.IsNullOrEmpty(searchString))
            {
                var strings = searchString.Split(' ');
                foreach (var splitString in strings)
                    auctions = auctions.Where(s => s.Name.Contains(splitString));
            }
            if (!String.IsNullOrEmpty(min))
            {
                double val = Double.Parse(min);
                auctions = auctions.Where(a => a.Price > val);
            }
            if (!String.IsNullOrEmpty(max))
            {
                double val = Double.Parse(max);
                auctions = auctions.Where(a => a.Price < val);
            }
            if (!String.IsNullOrEmpty(status))
            {
                AuctionState state = (AuctionState)Enum.Parse(typeof(AuctionState), status);
                auctions = auctions.Where(a => a.State == state);
            }

            auctions = auctions.OrderByDescending(a => a.OpenedTime).ThenBy(a => a.CreatedTime);

            int pageSize = 8;
            if (String.IsNullOrEmpty(searchString) && String.IsNullOrEmpty(min) && String.IsNullOrEmpty(max) && String.IsNullOrEmpty(status))
                pageSize = 4;
            int pageNumber = (page ?? 1);
            return View(auctions.ToPagedList(pageNumber, pageSize));
        }



//USER

        [System.Web.Mvc.Authorize(Roles = "User")]
        public string Bid(int id, string username)
        {
            if (System.Web.HttpContext.Current.User != null && System.Web.HttpContext.Current.User.IsInRole("User"))
            {
                Model1 dbUser = new Model1();
                AspNetUser user = dbUser.AspNetUsers.Where(u => u.UserName == username).FirstOrDefault();
                if (user.Tokens < 1)
                    return "No more tokens!";

                Auction auct = db.Auctions.Find(id);


                if (auct.State != AuctionState.Open || auct.ClosedTime <= DateTime.Now) return "Bid failed. Auction is no longer opened";

                Bid bid = new Bid();

                bid.CreatedTime = DateTime.Now;
                bid.IDAuction = auct;
                bid.IDUser = user.Id;

                auct.Bids.Add(bid);
                string beforeLastUser = auct.LastBidder;
                auct.LastBidder = username;
                auct.PriceGrowth = auct.PriceGrowth + 1;

                if ((auct.ClosedTime - DateTime.Now).Value.TotalSeconds <= 10)
                    auct.ClosedTime = DateTime.Now.AddSeconds(10);

                user.Tokens = user.Tokens - 1;
                try
                {
                    db.Bids.Add(bid);
                    db.Entry(auct).State = EntityState.Modified;
                    db.SaveChanges();

                    try { 
                    dbUser.Entry(user).State = EntityState.Modified;
                    dbUser.SaveChanges();
                    }
                    catch(DbUpdateConcurrencyException ex)
                    {
                        return "Build Failed. User concurrency error";
                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    db.Bids.Remove(bid);
                    auct.LastBidder = beforeLastUser;
                    auct.PriceGrowth = auct.PriceGrowth - 1;
                    user.Tokens = user.Tokens + 1;
                    db.Entry(auct).State = EntityState.Unchanged;
                    db.Entry(user).State = EntityState.Unchanged;
                    auct.Bids.Remove(bid);

                    return "Bid failed. Try again";
                }


                return "OK";
            }
            else return "Log in first!";

        }

        [System.Web.Mvc.Authorize(Roles = "User")]
        public ActionResult WonAuctions(int? page)
        {
            AspNetUser user = dbUser.AspNetUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            var auctions = (from a in db.Auctions
                            where a.Active == true
                            && a.State == AuctionState.Sold
                            select a).ToList();

            List<Auction> myAyctions = new List<Auction>();

            foreach (Auction a in auctions)
            {

                DateTime maxDate = a.Bids.Max(m => m.CreatedTime);
                Bid bid = a.Bids.Where(m => m.CreatedTime == maxDate).FirstOrDefault();
                if (bid.IDUser == user.Id)
                {
                    myAyctions.Add(a);
                }
            }

            int pageSize = 8;
            int pageNumber = (page ?? 1);
            return View(myAyctions.ToPagedList(pageNumber, pageSize));
        }


//ADMIN
        [System.Web.Mvc.Authorize(Roles = "Admin")]
        public ActionResult Auctions(int? page)
        {
            //checkState();
            var auctions = from a in db.Auctions
                           where a.Active == true
                           select a;

            auctions = auctions.OrderByDescending(a => a.CreatedTime);

            int pageSize = 8;
            int pageNumber = (page ?? 1);
            return View(auctions.ToPagedList(pageNumber, pageSize));
        }

        public void checkState()
        {
            var auctions = from a in db.Auctions
                           where a.Active == true
                           && a.State == AuctionState.Open
                           && a.ClosedTime <= DateTime.Now
                           select a;
            foreach (Auction a in auctions)
            {
                if (a.Bids != null && a.Bids.Count > 0)
                    a.State = AuctionState.Sold;
                else
                    a.State = AuctionState.Expired;
                db.Entry(a).State = EntityState.Modified;
            }
            db.SaveChanges();
        }

        // GET: Auctions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = db.Auctions.Find(id);
            if (auction == null)
            {
                return HttpNotFound();
            }
            return View(auction);
        }

        // GET: Auctions/Create
        [System.Web.Mvc.Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Auctions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [System.Web.Mvc.Authorize(Roles = "Admin")]
        public ActionResult Create(AuctionCreate auctionData)
        {
            if (ModelState.IsValid)
            {
                Auction auction = new Auction();

                auction.Name = auctionData.Name;
                auction.Time = auctionData.Time;
                auction.Price = auctionData.Price;

                auction.CreatedTime = DateTime.Now;
                auction.OpenedTime = null;
                auction.ClosedTime = null;
                auction.State = AuctionState.Draft;
                auction.Active = true;
                auction.PriceGrowth = 0;

                auction.Image = new byte[auctionData.ImageToUpload.ContentLength];
                auctionData.ImageToUpload.InputStream.Read(auction.Image, 0, auction.Image.Length);

                db.Auctions.Add(auction);
                db.SaveChanges();
                return RedirectToAction("Auctions");
            }

            return View(auctionData);
        }

        // GET: Auctions/Edit/5
        [System.Web.Mvc.Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = db.Auctions.Find(id);
            if (auction == null)
            {
                return HttpNotFound();
            }
            return View(auction);
        }

        // POST: Auctions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [System.Web.Mvc.Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "ID,Name,Time,Price,CreatedTime,OpenedTime,ClosedTime,Active,State,PriceGrowth,Image")] Auction auction)
        {
            if (ModelState.IsValid)
            {
                if (auction.State == AuctionState.Ready)
                {
                    db.Entry(auction).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Auctions");
                }
            }
            return View(auction);
        }

        // GET: Auctions/Delete/5
        [System.Web.Mvc.Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = db.Auctions.Find(id);
            if (auction == null)
            {
                return HttpNotFound();
            }
            return View(auction);
        }

        // POST: Auctions/Delete/5
        [System.Web.Mvc.Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Auction auction = db.Auctions.Find(id);
            if (auction.State == AuctionState.Ready)
            {
                auction.Active = false;
                db.Entry(auction).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Auctions");
        }

        // GET: Auctions/Ready/5
        [System.Web.Mvc.Authorize(Roles = "Admin")]
        public ActionResult Ready(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = db.Auctions.Find(id);
            if (auction == null)
            {
                return HttpNotFound();
            }
            if (auction.State == AuctionState.Draft)
            {
                auction.State = AuctionState.Ready;

                db.Entry(auction).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Auctions");
        }

        // GET: Auctions/Open/5
        [System.Web.Mvc.Authorize(Roles = "Admin")]
        public ActionResult Open(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = db.Auctions.Find(id);
            if (auction == null)
            {
                return HttpNotFound();
            }
            if (auction.State == AuctionState.Ready)
            {
                auction.State = AuctionState.Open;
                auction.OpenedTime = DateTime.Now;
                auction.ClosedTime = DateTime.Now.AddSeconds(auction.Time);

                db.Entry(auction).State = EntityState.Modified;
                db.SaveChanges();

                Task.Factory.StartNew(() =>
                {
                    ApplicationDbContext newDB = new ApplicationDbContext();
                    Auction task = null;
                    Boolean finished = false;
                    while (!finished)
                    {
                        newDB = new ApplicationDbContext();
                        task = newDB.Auctions.Find(id);
                        int time = (int)(task.ClosedTime - DateTime.Now).Value.TotalSeconds;
                        if (time > 0)
                            Thread.Sleep(time * 1000);
                        else
                            finished = true;
                    }

                    if (task.Bids.Count > 0)
                        task.State = AuctionState.Sold;
                    else
                        task.State = AuctionState.Expired;

                    newDB.Entry(task).State = EntityState.Modified;
                    newDB.SaveChanges();

                    var hub = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                    hub.Clients.All.closeAuction(id);
                });
            }
            return RedirectToAction("Auctions");
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        [System.Web.Mvc.Authorize(Roles = "Admin")]
        public ActionResult ChangePrice(int startingPrice, int id)
        {
            ChangePriceViewModel cpvm = new ChangePriceViewModel();
            cpvm.ID = id;
            cpvm.Price = startingPrice;

            return View(cpvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePrice(ChangePriceViewModel model)
        {

            if (ModelState.IsValid)
            {
                Auction auction = db.Auctions.Find(model.ID);
                auction.Price = model.Price;
                db.Entry(auction).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Auctions");
            }
            return View(model);
        }
    }
}
