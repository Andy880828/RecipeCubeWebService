using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace RecipeCubeWebService.DTO
{
    public class RecipeDto
    {
        public int recipeId { get; set; }

        [Required(ErrorMessage = "食譜名稱為必填")]
        [MaxLength(10, ErrorMessage = "食譜名稱長度不可超過10個字")]
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
        public string? userName { get; set; }

        public string? description { get; set; }
        // 使用者選擇的食材清單
        [Required(ErrorMessage = "請選擇至少一種食材")]
        public List<int> selectedIngredients { get; set; } = new List<int>();
        // 食材名稱
        public List<string> selectedIngredientNames { get; set; } = new List<string>();
        // 新增同義字欄位
        public List<string> synonyms { get; set; } = new List<string>(); 
        // 食材數量
        [Required(ErrorMessage = "請填寫食材數量")]
        public Dictionary<int, decimal> ingredientQuantities { get; set; } = new Dictionary<int, decimal>();

        // 食材的單位
        public Dictionary<int, string> ingredientUnits { get; set; } = new Dictionary<int, string>();
    }

}
