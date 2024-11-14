
namespace RecipeCubeWebService.DTO
{
    public class RandomRecommendReciepDTO
    {
        public int recipeId { get; set; }
        public string recipeName { get; set; }

        public string userId { get; set; }

        public bool? isCustom { get; set; }

        public bool? restriction { get; set; }

        public bool? westEast { get; set; }

        public string category { get; set; }

        public string detailedCategory { get; set; }

        public string steps { get; set; }

        public string seasoning { get; set; }

        public int? visibility { get; set; }

        public string? photoName { get; set; }

        public bool status { get; set; }

        public string time { get; set; }

        public string? description { get; set; }

        // 食材名稱
        public List<string> selectedIngredientNames { get; set; } = new List<string>();

        // 食材數量
        public Dictionary<int, decimal> ingredientQuantities { get; set; } = new Dictionary<int, decimal>();

        // 食材的單位
        public Dictionary<int, string> ingredientUnits { get; set; } = new Dictionary<int, string>();

        // 缺少的食材
        public List<MissingIngredientDTO>? missingIngredients { get; set; } = new List<MissingIngredientDTO>();

    }

}
