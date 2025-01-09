using System.ComponentModel;

namespace WebsiteSellingBonsaiAPI.Models
{
    public class BaseModel
    {
        [DisplayName("Create Data")]
        public DateTime? CreatedDate { get; set; }
        [DisplayName("Create By")]
        public string? CreatedBy { get; set; }
        [DisplayName("Update Data")]
        public DateTime? UpdatedDate { get; set; }
        [DisplayName("Update By")]
        public string? UpdatedBy { get; set; }
    }
}
