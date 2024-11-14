using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeCubeWebService.DTO;
using RecipeCubeWebService.Models;

namespace RecipeCubeWebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly RecipeCubeContext _context;

        public RecipesController(RecipeCubeContext context)
        {
            _context = context;
        }

        //// GET: api/Recipes
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Recipe>>> GetRecipes()
        //{
        //    return await _context.Recipes.ToListAsync();
        //}
        // GET: api/Recipes
        // GET: api/Recipes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecipeDto>>> GetRecipes()
        {
            var recipes = await _context.recipes
                .Where(r => r.status == true) // 只取狀態為 true 的食譜
                .ToListAsync();

            var recipeDtos = new List<RecipeDto>();
            foreach (var recipe in recipes)
            {
                // 手動查詢與該 Recipe 關聯的 RecipeIngredients
                var recipeIngredients = await _context.recipe_ingredients
                    .Where(ri => ri.recipeId == recipe.recipeId)
                    .ToListAsync();
                // 查詢 UserName
                var user = await _context.user.FirstOrDefaultAsync(u => u.Id == recipe.userId);
                string userName = user?.UserName ?? "系統"; // 如果找不到則設為 "系統"
                // 查詢對應的食材
                var ingredientIds = recipeIngredients.Select(ri => ri.ingredientId).Distinct().ToList();
                var ingredients = await _context.ingredients
                    .Where(i => ingredientIds.Contains(i.ingredientId))
                    .ToListAsync();

                // 查詢同義字
                var synonyms = ingredients.SelectMany(i => i.synonym?.Split(',') ?? Array.Empty<string>()).Distinct().ToList();

                // 將 SelectedIngredients 排序
                var sortedIngredientIds = recipeIngredients.Select(ri => ri.ingredientId ?? 0).ToList();
                sortedIngredientIds.Sort();

                // 創建 DTO
                var recipeDto = new RecipeDto
                {
                    recipeId = recipe.recipeId,
                    recipeName = recipe.recipeName,
                    userId = recipe.userId,
                    userName = userName,
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
                    selectedIngredients = sortedIngredientIds,
                    selectedIngredientNames = ingredients.Select(i => i.ingredientName).ToList(),
                    ingredientQuantities = recipeIngredients.GroupBy(ri => ri.ingredientId ?? 0)
                                                              .ToDictionary(g => g.Key, g => g.First().quantity ?? 0M),
                    ingredientUnits = ingredients.ToDictionary(i => i.ingredientId, i => i.unit ?? string.Empty),
                    synonyms = synonyms
                };

                recipeDtos.Add(recipeDto);
            }

            return Ok(recipeDtos);
        }

        // GET: api/Recipes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<RecipeDto>> GetRecipe(int id)
        {
            var recipe = await _context.recipes.FirstOrDefaultAsync(r => r.recipeId == id);
            if (recipe == null)
            {
                return NotFound();
            }
            // 查詢 UserName
            var user = await _context.user.FirstOrDefaultAsync(u => u.Id == recipe.userId);
            string userName = user?.UserName ?? "系統"; // 如果找不到則設為 "系統"
            // 手動查詢與該 Recipe 關聯的 RecipeIngredients
            var recipeIngredients = await _context.recipe_ingredients
                .Where(ri => ri.recipeId == recipe.recipeId)
                .ToListAsync();

            // 查詢對應的食材
            var ingredientIds = recipeIngredients.Select(ri => ri.ingredientId).Distinct().ToList();
            var ingredients = await _context.ingredients
                .Where(i => ingredientIds.Contains(i.ingredientId))
                .ToListAsync();

            // 查詢同義字
            var synonyms = ingredients.SelectMany(i => i.synonym?.Split(',') ?? Array.Empty<string>()).Distinct().ToList();

            var sortedIngredientIds = recipeIngredients.Select(ri => ri.ingredientId ?? 0).ToList();
            sortedIngredientIds.Sort();

            // 創建 DTO
            var recipeDto = new RecipeDto
            {
                recipeId = recipe.recipeId,
                recipeName = recipe.recipeName,
                userId = recipe.userId,
                userName = userName,
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
                selectedIngredients = sortedIngredientIds,
                selectedIngredientNames = ingredients.Select(i => i.ingredientName).ToList(),
                ingredientQuantities = recipeIngredients.GroupBy(ri => ri.ingredientId ?? 0)
                                                          .ToDictionary(g => g.Key, g => g.First().quantity ?? 0M),
                ingredientUnits = ingredients.ToDictionary(i => i.ingredientId, i => i.unit ?? string.Empty),
                synonyms = synonyms
            };

            return Ok(recipeDto);
        }

        // PUT: api/Recipes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

        [HttpPut("{id}")]
        public async Task<IActionResult> PutRecipe(int id, [FromForm] RecipeDto recipeDto, IFormFile? photo)
        {
            if (id != recipeDto.recipeId)
            {

                return BadRequest(new { message = "修改食譜資訊失敗" });
            }

            var recipe = await _context.recipes.FirstOrDefaultAsync(r => r.recipeId == id);
            if (recipe == null)
            {
                return NotFound();
            }

            // Update recipe properties
            recipe.recipeName = recipeDto.recipeName;
            recipe.userId = recipeDto.userId;
            recipe.isCustom = recipeDto.isCustom;
            recipe.restriction = recipeDto.restriction;
            recipe.westEast = recipeDto.westEast;
            recipe.category = recipeDto.category;
            recipe.detailedCategory = recipeDto.detailedCategory;
            recipe.steps = recipeDto.steps;
            recipe.seasoning = recipeDto.seasoning;
            recipe.visibility = recipeDto.visibility;
            recipe.status = recipeDto.status;
            recipe.time = recipeDto.time;
            recipe.description = recipeDto.description;

            // 如果有上傳新的圖片，儲存圖片
            if (photo != null)
            {
                // 定義圖片儲存的路徑
                var imagePath = Path.Combine("wwwroot/images/recipe", photo.FileName);

                // 儲存圖片到指定位置
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                // 更新圖片名稱到資料庫
                recipe.photo = photo.FileName;
            }

            _context.Entry(recipe).State = EntityState.Modified;

            // Update RecipeIngredients
            var existingIngredients = await _context.recipe_ingredients
                .Where(ri => ri.recipeId == recipe.recipeId)
                .ToListAsync();

            // Remove ingredients that are no longer associated
            foreach (var existingIngredient in existingIngredients)
            {
                if (!recipeDto.selectedIngredients.Contains(existingIngredient.ingredientId ?? 0))
                {
                    _context.recipe_ingredients.Remove(existingIngredient);
                }
            }

            // Add or update ingredients
            foreach (var ingredientId in recipeDto.selectedIngredients)
            {
                var existingIngredient = existingIngredients.FirstOrDefault(ri => ri.ingredientId == ingredientId);
                if (existingIngredient == null)
                {
                    // Add new ingredient
                    var recipeIngredient = new recipe_ingredients
                    {
                        recipeId = recipe.recipeId,
                        ingredientId = ingredientId,
                        quantity = recipeDto.ingredientQuantities.ContainsKey(ingredientId) ? recipeDto.ingredientQuantities[ingredientId] : 0M
                    };
                    _context.recipe_ingredients.Add(recipeIngredient);
                }
                else
                {
                    // Update existing ingredient quantity
                    existingIngredient.quantity = recipeDto.ingredientQuantities.ContainsKey(ingredientId) ? recipeDto.ingredientQuantities[ingredientId] : 0M;
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RecipeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { message = "修改食譜資訊成功" });
        }


        // POST: api/Recipes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [RequestSizeLimit(104857600)] // 100 MB
        public async Task<ActionResult<recipes>> PostRecipe([FromForm] RecipeDto recipeDto, IFormFile photo)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { Errors = errors });
            }
            // Custom validation
            if (string.IsNullOrWhiteSpace(recipeDto.recipeName))
            {
                return BadRequest("食譜名稱不能為空");
            }

            if (recipeDto.selectedIngredients == null || !recipeDto.selectedIngredients.Any())
            {
                return BadRequest("至少選擇一樣食材");
            }

            if (string.IsNullOrWhiteSpace(recipeDto.category))
            {
                return BadRequest("類別需被選擇");
            }
            // Save photo
            string photoFileName = null;
            if (photo != null && photo.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/recipe");
                photoFileName = $"{Guid.NewGuid()}_{Path.GetFileName(photo.FileName).Replace(" ", "_")}";
                string filePath = Path.Combine(uploadsFolder, photoFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }
            }
            if (photo == null)
            {
                return BadRequest("必須上傳一張食譜照片");
            }

            Console.WriteLine(photoFileName);
            // Create new Recipe entity
            var recipe = new recipes
            {
                recipeName = recipeDto.recipeName,
                userId = recipeDto.userId,
                isCustom = recipeDto.isCustom,
                restriction = recipeDto.restriction,
                westEast = recipeDto.westEast,
                category = recipeDto.category,
                detailedCategory = recipeDto.detailedCategory,
                steps = recipeDto.steps,
                seasoning = recipeDto.seasoning,
                visibility = recipeDto.visibility,
                photo = photoFileName,
                status = recipeDto.status,
                time = recipeDto.time,
                description = recipeDto.description,
            };

            _context.recipes.Add(recipe);
            await _context.SaveChangesAsync();

            // Handle selected ingredients and their quantities
            if (recipeDto.selectedIngredients != null && recipeDto.ingredientQuantities != null)
            {
                var ingredientIds = recipeDto.selectedIngredients.Distinct().ToList();

                foreach (var ingredientId in ingredientIds)
                {
                    if (recipeDto.ingredientQuantities.ContainsKey(ingredientId))
                    {
                        var quantity = recipeDto.ingredientQuantities[ingredientId];

                        var recipeIngredient = new recipe_ingredients
                        {
                            recipeId = recipe.recipeId,
                            ingredientId = ingredientId,
                            quantity = quantity
                        };
                        _context.recipe_ingredients.Add(recipeIngredient);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return CreatedAtAction("GetRecipe", new { id = recipe.recipeId }, recipe);
        }

        // DELETE: api/Recipes/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteRecipe(int id)
        //{
        //    var recipe = await _context.Recipes.FindAsync(id);
        //    if (recipe == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Recipes.Remove(recipe);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}
        // PATCH: api/Recipes/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateRecipeStatus(int id, [FromBody] RecipeStatusDto recipeStatusDto)
        {
            var recipe = await _context.recipes.FirstOrDefaultAsync(r => r.recipeId == id);
            if (recipe == null)
            {
                return NotFound(new { message = "找不到該食譜" });
            }

            // 更新狀態
            recipe.status = recipeStatusDto.status;

            // 將更新的狀態應用到資料庫中
            try
            {
                _context.Entry(recipe).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RecipeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { message = "成功更新食譜狀態" });
        }
        private bool RecipeExists(int id)
        {
            return _context.recipes.Any(e => e.recipeId == id);
        }

    }
}
