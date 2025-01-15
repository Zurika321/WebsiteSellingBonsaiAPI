namespace WebsiteSellingBonsaiAPI.DTOS.User
{
    public interface IUrlService
    {
        public string? GenerateUrl(
            string action = "Index",
            string controller = "Home",
            object values = null,
            string area = null,
            string scheme = null);
    }
}
