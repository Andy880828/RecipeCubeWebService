namespace RecipeCubeWebService.DTO
{
    public class RecommendRecipeRequestDTO
    {
        public string userId { get; set; }
        public List<int> selectedIngredients { get; set; }
    }
}