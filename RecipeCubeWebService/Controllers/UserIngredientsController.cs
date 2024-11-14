using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeCubeWebService.DTO;
using RecipeCubeWebService.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RecipeCubeWebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserIngredientsController : ControllerBase
    {
        private readonly RecipeCubeContext _context;

        public UserIngredientsController(RecipeCubeContext context)
        {
            _context = context;
        }

        // 取得使用者不可食用食材
        // GET: /api/UserIngredients/ExclusiveIngredientsName
        [HttpGet("ExclusiveIngredientsName")]
        public IActionResult GetExclusiveIngredientDetails([FromQuery] Users_Id_IngredientsDTO exi)
        {
            var exclusiveIngredientsDetails = _context.exclusive_ingredients
            .Where(e => e.userId == exi.user_Id)
            .Select(e => new
            {
                IngredientId = e.ingredientId,
                ExclusiveIngredientId = e.exclusiveIngredientId,
            })
            .ToList();
            if (!exclusiveIngredientsDetails.Any())
            {
                return NoContent(); // 或者使用 return Ok(); 返回空的結果
            }
            return Ok(new { ExclusiveIngredients = exclusiveIngredientsDetails });
        }

        // 取得使用者偏好食材
        // GET: /api/UserIngredients/PreferedIngrediensName
        [HttpGet("PreferedIngredientsName")]
        public IActionResult GetPreferedIngredientDetails([FromQuery] Users_Id_IngredientsDTO exi)
        {
            var preferredIngredientsDetails = _context.prefered_ingredients
            .Where(e => e.userId == exi.user_Id)
            .Select(e => new
            {
                IngredientId = e.ingredientId,
                PreferIngredientId = e.preferIngredientId,

            })
            .ToList();
            if (!preferredIngredientsDetails.Any())
            {
                return NoContent(); // 或者使用 return Ok(); 返回空的結果
            }
            return Ok(new { PreferredIngredients = preferredIngredientsDetails });
        }


        // POST: /api/UserIngredients/ExclusiveIngredientsAdd
        [HttpPost("ExclusiveIngredientsAdd")]
        public async Task<IActionResult> ExclusiveIngredientsAddDTO(IngredientsAddDTO ExclusiveIngredientsAdd)
        {
            if (ExclusiveIngredientsAdd == null)
            {
                return BadRequest("Invalid signup data.");
            }
            var existingUser = await _context.user.SingleOrDefaultAsync(u => u.Id == ExclusiveIngredientsAdd.User_Id);
            if (existingUser == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            var userId = ExclusiveIngredientsAdd.User_Id;
            var IngredientsId = ExclusiveIngredientsAdd.Ingredient_Id;

            var newExclusiveIngredient = new exclusive_ingredients
            {
                userId = ExclusiveIngredientsAdd.User_Id,
                ingredientId = ExclusiveIngredientsAdd.Ingredient_Id
            };

            // 將新項目新增至資料庫
            _context.exclusive_ingredients.Add(newExclusiveIngredient);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "新增成功" });
        }


        // POST: /api/UserIngredients/PreferedIngredientsAdd
        [HttpPost("PreferedIngredientsAdd")]
        public async Task<IActionResult> PreferedIngredientsAddDTO(IngredientsAddDTO PreferedIngredientsAdd)
        {
            if (PreferedIngredientsAdd == null)
            {
                return BadRequest("Invalid signup data.");
            }
            var existingUser = await _context.user.SingleOrDefaultAsync(u => u.Id == PreferedIngredientsAdd.User_Id);
            if (existingUser == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            var userId = PreferedIngredientsAdd.User_Id;
            var IngredientsId = PreferedIngredientsAdd.Ingredient_Id;

            var newPreferedIngredient = new prefered_ingredients
            {
                userId = PreferedIngredientsAdd.User_Id,
                ingredientId = PreferedIngredientsAdd.Ingredient_Id
            };

            // 將新項目新增至資料庫
            _context.prefered_ingredients.Add(newPreferedIngredient);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "新增成功" });
        }

        // DELETE: /api/UserIngredients/ExclusiveIngredientsDelete
        [HttpDelete("ExclusiveIngredientsDelete")]
        public async Task<IActionResult> DeleteExclusiveIngredient(int id)
        {
            var exclusiveIngredient = await _context.exclusive_ingredients.FindAsync(id);
            if (exclusiveIngredient == null)
            {
                return NotFound();
            }

            _context.exclusive_ingredients.Remove(exclusiveIngredient);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: /api/UserIngredients/PreferedIngrediensDelete
        [HttpDelete("PreferedIngrediensDelete")]
        public async Task<IActionResult> DeletePreferedIngredient(int id)
        {
            var preferedIngredient = await _context.prefered_ingredients.FindAsync(id);
            if (preferedIngredient == null)
            {
                return NotFound();
            }

            _context.prefered_ingredients.Remove(preferedIngredient);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private bool ExclusiveIngredientExists(int id)
        {
            return _context.exclusive_ingredients.Any(e => e.exclusiveIngredientId == id);
        }
    }

}
