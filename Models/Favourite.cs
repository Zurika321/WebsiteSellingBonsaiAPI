using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebsiteSellingBonsaiAPI.Models;

namespace WebsiteSellingBonsaiAPI.Models
{
    [Table("Favourites")]
    public class Favourite
    {
        [Key]
        public int FAV_ID { get; set; }

        [ForeignKey("Bonsai")]
        public int BONSAI_ID { get; set; }

        [ForeignKey("ApplicationUser")]
        public string USE_ID { get; set; }

        public bool Fav { get; set; }

        // Navigation properties
        public virtual Bonsai? Bonsai { get; set; }
        public virtual ApplicationUser? ApplicationUser { get; set; }
    }
}
