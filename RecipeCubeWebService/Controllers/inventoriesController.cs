using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeCubeWebService.Models;
using RecipeCubeWebService.DTO;

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
            if (request == null || request.SelectedIngredients == null || request.SelectedIngredients.Count == 0)
            {
                return BadRequest(new { message = "食材 ID 列表不能為空" });
            }

            try
            {
                var userId = request.UserId.ToString();
                var selectedIngredients = request.SelectedIngredients;

                bool isVegetarian = false;
                var perferredIngredients = new List<int?>();
                var exclusiveIngredients = new List<int?>();
                List<Inventory> userInventory = new List<Inventory>();
                var groupUserIds = new List<string>();

                // 針對用戶條件的查詢
                if (userId != "0")
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null)
                    {
                        return NotFound(new { message = "找不到用戶" });
                    }

                    isVegetarian = user.DietaryRestrictions;
                    perferredIngredients = await _context.PreferedIngredients.Where(p => p.UserId == userId).Select(p => p.IngredientId).ToListAsync();
                    exclusiveIngredients = await _context.ExclusiveIngredients.Where(e => e.UserId == userId).Select(e => e.IngredientId).ToListAsync();
                    userInventory = await _context.Inventories.Where(inv => inv.UserId == userId).ToListAsync();
                    groupUserIds = await _context.Users
    .Where(u => u.GroupId == user.GroupId)
    .Select(u => u.Id)
    .ToListAsync();
                }

                var recipesQuery = _context.Recipes
                    .Where(r => r.Status == true)
                    .Where(r =>
                        r.Visibility == 0 ||
                        (r.Visibility == 1 && groupUserIds.Contains(r.UserId)) ||
                        (r.Visibility == 2 && r.UserId == userId));

                if (isVegetarian)
                {
                    recipesQuery = recipesQuery.Where(r => r.Restriction == true);
                }
                var recipes = await recipesQuery.ToListAsync();
                var recipeIds = recipes.Select(r => r.RecipeId).ToList();
                var recipeIngredients = await _context.RecipeIngredients
                    .Where(ri => recipeIds.Contains((int)ri.RecipeId))
                    .Join(_context.Ingredients,
                          recipeIngredient => recipeIngredient.IngredientId,
                          ingredient => ingredient.IngredientId,
                          (recipeIngredient, ingredient) => new
                          {
                              recipeIngredient.RecipeId,
                              recipeIngredient.IngredientId,
                              recipeIngredient.Quantity,
                              IngredientName = ingredient.IngredientName,
                              Unit = ingredient.Unit
                          })
                    .ToListAsync();

                var recommendedRecipes = new List<RecommendRecipeDTO>();

                foreach (var recipe in recipes)
                {
                    var currentRecipeIngredients = recipeIngredients
                        .Where(ri => ri.RecipeId == recipe.RecipeId)
                        .OrderBy(ri => ri.IngredientId)
                        .ToList();

                    var ingredientIds = currentRecipeIngredients.Select(ri => ri.IngredientId).ToList();
                    var ingredientNames = currentRecipeIngredients.Select(ri => ri.IngredientName).ToList();

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
                                var inventoryItem = userInventory.FirstOrDefault(inv => inv.IngredientId == recipeIngredient.IngredientId);
                                if (inventoryItem == null || inventoryItem.Quantity < recipeIngredient.Quantity)
                                {
                                    isEnoughIngredients = false;
                                    var missingQuantity = recipeIngredient.Quantity - (inventoryItem?.Quantity ?? 0);
                                    missingIngredients.Add(new MissingIngredientDTO
                                    {
                                        IngredientId = (int)recipeIngredient.IngredientId,
                                        IngredientName = recipeIngredient.IngredientName,
                                        MissingQuantity = missingQuantity,
                                        Unit = recipeIngredient.Unit
                                    });
                                }
                            }
                        }

                        recommendedRecipes.Add(new RecommendRecipeDTO
                        {
                            RecipeId = recipe.RecipeId,
                            RecipeName = recipe.RecipeName,
                            MatchRate = matchRate,
                            CoverRate = coverRate,
                            TotalScore = totalScore,
                            photoName = recipe.Photo,
                            Restriction = recipe.Restriction,
                            WestEast = recipe.WestEast,
                            Category = recipe.Category,
                            DetailedCategory = recipe.DetailedCategory,
                            IngredientIds = ingredientIds,
                            IngredientNames = ingredientNames,
                            IsEnoughIngredients = isEnoughIngredients,
                            MissingIngredients = missingIngredients
                        });
                    }
                }

                var sortedRecipes = recommendedRecipes
                    .Select(r =>
                    {
                        r.MatchRate = Math.Round(r.MatchRate, 3);
                        r.CoverRate = Math.Round(r.CoverRate, 3);
                        r.TotalScore = Math.Round(r.TotalScore, 3);
                        return r;
                    })
                    .OrderByDescending(r => r.IngredientIds.Count(id => perferredIngredients.Contains(id)))
                    .ThenByDescending(r => r.TotalScore)
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
                var allRecipes = await _context.Recipes.Where(r => r.Status == true).ToListAsync();

                // 如果有 userId，嘗試優先推薦偏好相關的食譜
                if (!string.IsNullOrEmpty(userId))
                {
                    var preferredIngredients = await _context.PreferedIngredients
                        .Where(pi => pi.UserId == userId)
                        .Select(pi => pi.IngredientId)
                        .ToListAsync();

                    var exclusiveIngredients = await _context.ExclusiveIngredients
                        .Where(ei => ei.UserId == userId)
                        .Select(ei => ei.IngredientId)
                        .ToListAsync();

                    // 過濾掉含有禁忌食材的食譜
                    var filteredRecipes = allRecipes
                        .Where(r => !_context.RecipeIngredients
                            .Any(ri => ri.RecipeId == r.RecipeId && exclusiveIngredients.Contains(ri.IngredientId)))
                        .ToList();

                    // 優先推薦包含偏好食材的食譜
                    var recommendedRecipes = filteredRecipes
                        .Where(r => _context.RecipeIngredients
                            .Any(ri => ri.RecipeId == r.RecipeId && preferredIngredients.Contains(ri.IngredientId)))
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

        private async Task<RandomRecommendReciepDTO> ConvertToDTO(Recipe recipe)
        {
            var recipeIngredients = await _context.RecipeIngredients
                .Where(ri => ri.RecipeId == recipe.RecipeId)
                .ToListAsync();

            var ingredients = await _context.Ingredients
                .ToListAsync();

            var selectedIngredientNames = recipeIngredients
                .Select(ri => ingredients.FirstOrDefault(i => i.IngredientId == ri.IngredientId)?.IngredientName ?? string.Empty)
                .ToList();


            return new RandomRecommendReciepDTO
            {
                RecipeId = recipe.RecipeId,
                RecipeName = recipe.RecipeName,
                UserId = recipe.UserId,
                IsCustom = recipe.IsCustom,
                Restriction = recipe.Restriction,
                WestEast = recipe.WestEast,
                Category = recipe.Category,
                DetailedCategory = recipe.DetailedCategory,
                Steps = recipe.Steps,
                Seasoning = recipe.Seasoning,
                Visibility = recipe.Visibility,
                PhotoName = recipe.Photo,
                Status = (bool)recipe.Status,
                Time = recipe.Time,
                Description = recipe.Description,
                SelectedIngredientNames = selectedIngredientNames,
                IngredientQuantities = recipeIngredients.GroupBy(ri => ri.IngredientId ?? 0)
                                                          .ToDictionary(g => g.Key, g => g.First().Quantity ?? 0M),
                IngredientUnits = ingredients.ToDictionary(i => i.IngredientId, i => i.Unit ?? string.Empty),
                MissingIngredients = new List<MissingIngredientDTO>()
            };
        }
    }

}
