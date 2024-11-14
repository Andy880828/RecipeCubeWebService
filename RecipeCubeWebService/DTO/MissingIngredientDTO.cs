namespace RecipeCubeWebService.DTO
{
    public class MissingIngredientDTO
    {
        public int ingredientId { get; set; }
        public string ingredientName { get; set; }
        public decimal? missingQuantity { get; set; }
        public string unit { get; set; }
    }
}
