namespace RecipeCubeWebService.DTO;

    public class GetOrderItemProductUnit
    {
        public long? orderId { get; set; }
        public int? productId { get; set; }

        public int? quantity { get; set; }

        public int? price { get; set; }

        public string productName { get; set; }

        public int? ingredientId { get; set; }

        public string photo { get; set; }

        public string? unit { get; set; }

        public decimal? unitQuantity { get; set; }
    }

