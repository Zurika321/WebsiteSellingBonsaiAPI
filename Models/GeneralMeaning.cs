﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteSellingBonsaiAPI.Models
{
    [Table("GeneralMeanings")]
    public class GeneralMeaning : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Column("Meaning")]
        [DisplayName("General Meaning")]
        public string Meaning { get; set; }

        public ICollection<Bonsai>? Bonsais { get; set; }
    }
}
