namespace RecipeCubeWebService.DTO
{
    public class ProductsCategoryDTO
    {
        public int productId { get; set; }

        public int? ingredientId { get; set; }

        public string? category { get; set; }

        public int count {  get; set; }
    }
}
