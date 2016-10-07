using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Net.Http;
//using System.Web.Http;

namespace bi130168d.Models
{
    [Table("Bids")]
    public class Bid
    {
        [Key]
        public int ID { get; set; }
        public DateTime CreatedTime { get; set; }

        public virtual Auction IDAuction { get; set; }

        [ForeignKey("ApplicationUser")]
        public String IDUser { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}