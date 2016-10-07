using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace bi130168d.Models
{
    public enum OrderState
    {
        Processing, Accepted, Denied
    }

    [Table("Orders")]
    public class Order
    {
        [Key]
        public int ID { get; set; }
        public int Tokens { get; set; }
        public double Price { get; set; }
        public OrderState? Status { get; set; }
        public DateTime CreatedTime { get; set; }

        [ForeignKey("ApplicationUser")]
        public String IDUser { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}