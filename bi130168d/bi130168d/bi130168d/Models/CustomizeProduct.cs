using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace bi130168d.Models
{
     public class ProductMetadata
     {
        public int ID { get; set; }
        [Display(Name = "Product name")]
        public string Name { get; set; }
        public long Time { get; set; }
        [Display(Name = "Starting price")]
        public int Price { get; set; }
        [Display(Name = "Creation date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy H:mm:s}", ApplyFormatInEditMode =true)]
        public DateTime? CreatedTime { get; set; }
        [Display(Name = "Opening date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy H:mm:s}", ApplyFormatInEditMode = true)]
        public DateTime? OpenedTime { get; set; }
        [Display(Name = "Closing date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy H:mm:s}", ApplyFormatInEditMode = true)]
        public DateTime? ClosedTime { get; set; }
        public bool Active { get; set; }
        public AuctionState? State { get; set; }
        public int PriceGrowth { get; set; }
        public byte[] Image { get; set; }
     }

     [MetadataType(typeof(ProductMetadata))]
     public partial class Auction
     {
        [NotMapped]
        public HttpPostedFileBase ImageToUpload { get; set; }
     }

    public partial class AuctionCreate
    {
        public int ID { get; set; }

        [Required]
        [Display(Name = "Product name")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Time")]
        public long Time { get; set; }

        [Required]
        [Display(Name = "Starting price")]
        public int Price { get; set; }

        [Display(Name = "Creation date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy H:mm:s}", ApplyFormatInEditMode = true)]
        public DateTime? CreatedTime { get; set; }

        [Display(Name = "Opening date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy H:mm:s}", ApplyFormatInEditMode = true)]
        public DateTime? OpenedTime { get; set; }

        [Display(Name = "Closing date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy H:mm:s}", ApplyFormatInEditMode = true)]
        public DateTime? ClosedTime { get; set; }

        public AuctionState? State { get; set; }
        
        public int PriceGrowth { get; set; }
        
        [Display(Name = "Image")]
        public byte[] Image { get; set; }

        [Required]
        [Display(Name = "Image")]
        public HttpPostedFileBase ImageToUpload { get; set; }

    }

    public class ChangePriceViewModel
    {
        [Required]
        [Display(Name = "Price")]
        public int Price { get; set; }

        public int ID { get; set; }
    }

}