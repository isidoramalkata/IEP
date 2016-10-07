using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bi130168d.Models
{
    public enum AuctionState
    {
        Draft, Ready, Open, Sold, Expired
    }

    [Table("Auctions")]
    public partial class Auction
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public long Time { get; set; }
        public int Price { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? OpenedTime { get; set; }
        public DateTime? ClosedTime { get; set; }
        public bool Active { get; set; }
        public AuctionState? State { get; set; }
        public int PriceGrowth { get; set; }
        public byte[] Image { get; set; }
        
        public virtual ICollection<Bid> Bids { get; set; }

        public string LastBidder { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}