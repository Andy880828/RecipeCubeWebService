namespace RecipeCubeWebService.DTO;

    public class OrderItemDTO
    {
        public long? orderId { get; set; }

        public int? productId { get; set; }

        public int? quantity { get; set; }

        public int? price { get; set; }
    }

