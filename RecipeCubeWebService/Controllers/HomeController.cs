using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeCubeWebService.Models;
using RecipeCubeWebService.DTO;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RecipeCubeWebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly RecipeCubeContext _context;

        public HomeController(RecipeCubeContext context)
        {
            _context = context;
        }

        // GET: api/<HomeController>
        [HttpGet]
        public async Task<ActionResult<homeDTO>> GetInfo()
        {
            int recipeAmount = await _context.recipes.CountAsync();
            int ingredientAmount = await _context.ingredients.CountAsync();
            int GroupAmount = await _context.user_groups.CountAsync();
            int UserAmount = await _context.user.CountAsync();
            homeDTO homeDTO = new homeDTO
            {
                recipeAmount = recipeAmount,
                ingredientAmount = ingredientAmount,
                groupAmount = GroupAmount,
                userAmount = UserAmount
            };
            return homeDTO;
        }

        // GET: api/Home/Recommend
        [HttpGet("Recommend")]
        public async Task<ActionResult<IEnumerable<recipes>>> GetRecommendRecipe()
        {
            var recipes = await _context.recipes
                .Where(recipe => recipe.userId == "0" && recipe.status == true)
                .ToListAsync();

            int recipeAmount = recipes.Count;
            List<recipes> selectedRecipes = new List<recipes>();
            Random random = new Random();
            HashSet<int> selectedIndices = new HashSet<int>();

            // 隨機選取5個食譜
            while (selectedIndices.Count < 5 && selectedIndices.Count < recipeAmount)
            {
                int randomIndex = random.Next(0, recipeAmount);
                selectedIndices.Add(randomIndex);
            }

            // 根據隨機索引選擇食譜
            foreach (int index in selectedIndices)
            {
                selectedRecipes.Add(recipes[index]);
            }

            return Ok(selectedRecipes);
        }
    }

}
