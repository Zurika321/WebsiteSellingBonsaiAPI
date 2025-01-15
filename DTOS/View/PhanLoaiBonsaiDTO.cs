using WebsiteSellingBonsaiAPI.Models;

namespace WebsiteSellingBonsaiAPI.DTOS.View
{
    public class PhanLoaiBonsaiDTO
    {
        public IEnumerable<Style>? Styles { get; set; }
        public IEnumerable<BonsaiType>? Types { get; set; }
        public IEnumerable<GeneralMeaning>? GeneralMeanings { get; set; }
    }

}
