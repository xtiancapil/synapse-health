namespace DeliveryNotifier.Models
{
    public class OrderItem
    {
        public Guid ItemId { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int DeliveryNotification { get; set; }
    }
}
