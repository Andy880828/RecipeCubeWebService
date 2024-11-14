namespace RecipeCubeWebService.DTO
{
    public class RecommendRecipeDTO
    {
        public int recipeId { get; set; }
        public string recipeName { get; set; }
        public double matchRate { get; set; }
        public double coverRate { get; set; }
        public double totalScore { get; set; }
        public string photoName { get; set; }
        public bool? restriction { get; set; }
        public bool? westEast { get; set; }
        public string category { get; set; }
        public string detailedCategory { get; set; }
        public List<int?>? ingredientIds { get; set; }
        public List<string> ingredientNames { get; set; }
        public bool isEnoughIngredients { get; set; }  // 是否有足夠食材
        public List<MissingIngredientDTO>? missingIngredients { get; set; } // 缺少的食材及其數量與單位
    }
}
