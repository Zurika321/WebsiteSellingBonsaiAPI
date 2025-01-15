using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WebsiteSellingBonsaiAPI.Models;
using System.ComponentModel;
using Microsoft.AspNetCore.Http.HttpResults;

namespace WebsiteSellingBonsaiAPI.DTOS.View
{
    public class BonsaiDetailViewModel
    {
        public BonsaiDTO CurrentBonsai { get; set; }
        public List<BonsaiDTO> BonsaiRelatedMeaning { get; set; }
        public List<BonsaiDTO> BonsaiRelatedStyle { get; set; }
        public List<BonsaiDTO> BonsaiRelatedType { get; set; }
    }
}
