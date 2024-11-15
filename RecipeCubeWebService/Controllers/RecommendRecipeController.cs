using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeCubeWebService.DTO;
using RecipeCubeWebService.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RecipeCubeWebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendRecipeController : ControllerBase
    {
        private readonly RecipeCubeContext _context;

        public RecommendRecipeController(RecipeCubeContext context)
        {
            _context = context;
        }

        [HttpPost("Recommend")]
        public async Task<ActionResult<IEnumerable<RecommendRecipeDTO>>> RecommendRecipes([FromBody] RecommendRecipeRequestDTO request)
        {
            if (request == null || request.selectedIngredients == null || request.selectedIngredients.Count == 0)
            {
                return BadRequest(new { message = "食材 ID 列表不能為空" });
            }

            try
            {
                var userId = request.userId.ToString();
                var selectedIngredients = request.selectedIngredients;

                bool isVegetarian = false;
                var perferredIngredients = new List<int?>();
                var exclusiveIngredients = new List<int?>();
                List<inventory> userInventory = new List<inventory>();
                var groupUserIds = new List<string>();

                // 針對用戶條件的查詢
                if (userId != "0")
                {
                    var user = await _context.user.FindAsync(userId);
                    if (user == null)
                    {
                        return NotFound(new { message = "找不到用戶" });
                    }

                    isVegetarian = user.dietary_restrictions;
                    perferredIngredients = await _context.prefered_ingredients.Where(p => p.userId == userId).Select(p => p.ingredientId).ToListAsync();
                    exclusiveIngredients = await _context.exclusive_ingredients.Where(e => e.userId == userId).Select(e => e.ingredientId).ToListAsync();
                    userInventory = await _context.inventory.Where(inv => inv.userId == userId).ToListAsync();
                    groupUserIds = await _context.user
    .Where(u => u.groupId == user.groupId)
    .Select(u => u.Id)
    .ToListAsync();
                }

                var recipesQuery = _context.recipes
                    .Where(r => r.status == true)
                    .Where(r =>
                        r.visibility == 0 ||
                        (r.visibility == 1 && groupUserIds.Contains(r.userId)) ||
                        (r.visibility == 2 && r.userId == userId));

                if (isVegetarian)
                {
                    recipesQuery = recipesQuery.Where(r => r.restriction == true);
                }
                var recipes = await recipesQuery.ToListAsync();
                var recipeIds = recipes.Select(r => r.recipeId).ToList();
                var recipeIngredients = await _context.recipe_ingredients
                    .Where(ri => recipeIds.Contains((int)ri.recipeId))
                    .Join(_context.ingredients,
                          recipeIngredient => recipeIngredient.ingredientId,
                          ingredient => ingredient.ingredientId,
                          (recipeIngredient, ingredient) => new
                          {
                              recipeIngredient.recipeId,
                              recipeIngredient.ingredientId,
                              recipeIngredient.quantity,
                              ingredientName = ingredient.ingredientName,
                              unit = ingredient.unit
                          })
                    .ToListAsync();

                var recommendedRecipes = new List<RecommendRecipeDTO>();

                foreach (var recipe in recipes)
                {
                    var currentRecipeIngredients = recipeIngredients
                        .Where(ri => ri.recipeId == recipe.recipeId)
                        .OrderBy(ri => ri.ingredientId)
                        .ToList();

                    var ingredientIds = currentRecipeIngredients.Select(ri => ri.ingredientId).ToList();
                    var ingredientNames = currentRecipeIngredients.Select(ri => ri.ingredientName).ToList();

                    if (userId != "0" && ingredientIds.Any(id => exclusiveIngredients.Contains(id)))
                    {
                        continue;
                    }

                    var matchCount = selectedIngredients.Cast<int?>()
                        .Intersect(ingredientIds.Cast<int?>())
                        .Count();

                    if (matchCount > 0)
                    {
                        var matchRate = (double)matchCount / currentRecipeIngredients.Count;
                        var coverRate = (double)matchCount / selectedIngredients.Count;
                        var totalScore = matchRate * 0.5 + coverRate * 0.5;

                        bool isEnoughIngredients = true;
                        var missingIngredients = new List<MissingIngredientDTO>();

                        if (userId != "0")
                        {
                            foreach (var recipeIngredient in currentRecipeIngredients)
                            {
                                var inventoryItem = userInventory.FirstOrDefault(inv => inv.ingredientId == recipeIngredient.ingredientId);
                                if (inventoryItem == null || inventoryItem.quantity < recipeIngredient.quantity)
                                {
                                    isEnoughIngredients = false;
                                    var missingQuantity = recipeIngredient.quantity - (inventoryItem?.quantity ?? 0);
                                    missingIngredients.Add(new MissingIngredientDTO
                                    {
                                        ingredientId = (int)recipeIngredient.ingredientId,
                                        ingredientName = recipeIngredient.ingredientName,
                                        missingQuantity = missingQuantity,
                                        unit = recipeIngredient.unit
                                    });
                                }
                            }
                        }

                        recommendedRecipes.Add(new RecommendRecipeDTO
                        {
                            recipeId = recipe.recipeId,
                            recipeName = recipe.recipeName,
                            matchRate = matchRate,
                            coverRate = coverRate,
                            totalScore = totalScore,
                            photoName = recipe.photo,
                            restriction = recipe.restriction,
                            westEast = recipe.westEast,
                            category = recipe.category,
                            detailedCategory = recipe.detailedCategory,
                            ingredientIds = ingredientIds,
                            ingredientNames = ingredientNames,
                            isEnoughIngredients = isEnoughIngredients,
                            missingIngredients = missingIngredients
                        });
                    }
                }

                var sortedRecipes = recommendedRecipes
                    .Select(r =>
                    {
                        r.matchRate = Math.Round(r.matchRate, 3);
                        r.coverRate = Math.Round(r.coverRate, 3);
                        r.totalScore = Math.Round(r.totalScore, 3);
                        return r;
                    })
                    .OrderByDescending(r => r.ingredientIds.Count(id => perferredIngredients.Contains(id)))
                    .ThenByDescending(r => r.totalScore)
                    .ToList();

                if (!sortedRecipes.Any())
                {
                    return NotFound(new { message = "未找到符合條件的食譜" });
                }

                return Ok(sortedRecipes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "發生伺服器錯誤", error = ex.Message });
            }
        }


        [HttpGet("RandomRecommend")]
        public async Task<ActionResult<RandomRecommendReciepDTO>> RandomRecommend(string? userId)
        {
            try
            {
                // 查找所有食譜
                var allRecipes = await _context.recipes.Where(r => r.status == true).ToListAsync();

                // 如果有 userId，嘗試優先推薦偏好相關的食譜
                if (!string.IsNullOrEmpty(userId))
                {
                    var preferredIngredients = await _context.prefered_ingredients
                        .Where(pi => pi.userId == userId)
                        .Select(pi => pi.ingredientId)
                        .ToListAsync();

                    var exclusiveIngredients = await _context.exclusive_ingredients
                        .Where(ei => ei.userId == userId)
                        .Select(ei => ei.ingredientId)
                        .ToListAsync();

                    // 過濾掉含有禁忌食材的食譜
                    var filteredRecipes = allRecipes
                        .Where(r => !_context.recipe_ingredients
                            .Any(ri => ri.recipeId == r.recipeId && exclusiveIngredients.Contains(ri.ingredientId)))
                        .ToList();

                    // 優先推薦包含偏好食材的食譜
                    var recommendedRecipes = filteredRecipes
                        .Where(r => _context.recipe_ingredients
                            .Any(ri => ri.recipeId == r.recipeId && preferredIngredients.Contains(ri.ingredientId)))
                        .ToList();

                    if (recommendedRecipes.Any())
                    {
                        var randomRecommendedRecipe = recommendedRecipes
                            .OrderBy(r => Guid.NewGuid())
                            .FirstOrDefault();

                        return Ok(await ConvertToDTO(randomRecommendedRecipe));
                    }
                }

                // 如果沒有偏好或禁忌設定，或者沒有符合偏好的食譜，則隨機推薦
                var randomRecipe = allRecipes.OrderBy(r => Guid.NewGuid()).FirstOrDefault();

                if (randomRecipe == null)
                {
                    return NotFound(new { message = "未找到任何食譜" });
                }

                return Ok(await ConvertToDTO(randomRecipe));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "伺服器錯誤", error = ex.Message });
            }
        }

        private async Task<RandomRecommendReciepDTO> ConvertToDTO(recipes recipe)
        {
            var recipeIngredients = await _context.recipe_ingredients
                .Where(ri => ri.recipeId == recipe.recipeId)
                .ToListAsync();

            var ingredients = await _context.ingredients
                .ToListAsync();

            var selectedIngredientNames = recipeIngredients
                .Select(ri => ingredients.FirstOrDefault(i => i.ingredientId == ri.ingredientId)?.ingredientName ?? string.Empty)
                .ToList();


            return new RandomRecommendReciepDTO
            {
                recipeId = recipe.recipeId,
                recipeName = recipe.recipeName,
                userId = recipe.userId,
                isCustom = recipe.isCustom,
                restriction = recipe.restriction,
                westEast = recipe.westEast,
                category = recipe.category,
                detailedCategory = recipe.detailedCategory,
                steps = recipe.steps,
                seasoning = recipe.seasoning,
                visibility = recipe.visibility,
                photoName = recipe.photo,
                status = (bool)recipe.status,
                time = recipe.time,
                description = recipe.description,
                selectedIngredientNames = selectedIngredientNames,
                ingredientQuantities = recipeIngredients.GroupBy(ri => ri.ingredientId ?? 0)
                                                          .ToDictionary(g => g.Key, g => g.First().quantity ?? 0M),
                ingredientUnits = ingredients.ToDictionary(i => i.ingredientId, i => i.unit ?? string.Empty),
                missingIngredients = new List<MissingIngredientDTO>()
            };

        }
    }
}
