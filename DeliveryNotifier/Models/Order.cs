namespace DeliveryNotifier.Models
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
