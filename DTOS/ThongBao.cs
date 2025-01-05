namespace WebsiteSellingBonsaiAPI.DTOS
{
    public class ThongBao
    {
        public string Message { get; set; }
        public string MessageType { get; set; }
        public int DisplayTime { get; set; }
    }
    public class TypeThongBao
    {
        public const string Danger = "Danger";
        public const string Warning = "Warning";
        public const string Primary = "Primary";
        public const string Success = "Success";
    }
}
