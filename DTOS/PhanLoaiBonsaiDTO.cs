using WebsiteSellingBonsaiAPI.Models;

namespace WebsiteSellingBonsaiAPI.DTOS
{
    public class PhanLoaiBonsaiDTO
    {
        public IEnumerable<Style>? Styles { get; set; }
        public IEnumerable<BonsaiType>? Types { get; set; }
        public IEnumerable<GeneralMeaning>? GeneralMeanings { get; set; }
    }

}
