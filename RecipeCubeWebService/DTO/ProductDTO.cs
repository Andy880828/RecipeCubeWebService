namespace RecipeCubeWebService.DTO
{
    public class ProductDTO
    {
        public int productId { get; set; }

        public string productName { get; set; }

        public int? ingredientId { get; set; }

        public int? price { get; set; }

        public int? stock { get; set; }

        public bool? status { get; set; }

        public string photo { get; set; }

        public string? category { get; set; }

        public string? unit { get; set; }

        public decimal? unitQuantity { get; set; }

        public string? description { get; set; }
    }
}
