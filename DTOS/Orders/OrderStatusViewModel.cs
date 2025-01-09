using WebsiteSellingBonsaiAPI.Models;
namespace WebsiteSellingBonsaiAPI.DTOS.Orders
{
    public class OrderStatusViewModel
    {
        public IEnumerable<Order> NotConfirmedOrders { get; set; }
        public IEnumerable<Order> OnDeliveryOrders { get; set; }
        public IEnumerable<Order> OrderCompletedOrders { get; set; }
        public IEnumerable<Order> OrderCancelledOrders { get; set; }
    }

}
