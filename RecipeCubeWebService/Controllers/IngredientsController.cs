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
    public class IngredientsController : ControllerBase
    {
        private readonly RecipeCubeContext _context;

        public IngredientsController(RecipeCubeContext context)
        {
            _context = context;
        }

        // GET: api/Ingredients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ingredients>>> GetIngredients()
        {
            return await _context.ingredients.ToListAsync();
        }

        // GET: api/Ingredients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ingredients>> GetIngredient(int id)
        {
            var ingredient = await _context.ingredients.FindAsync(id);

            if (ingredient == null)
            {
                return NotFound();
            }

            return ingredient;
        }

        // PUT: api/Ingredients/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIngredient(int id, ingredients ingredient)
        {
            if (id != ingredient.ingredientId)
            {
                return BadRequest();
            }

            _context.Entry(ingredient).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IngredientExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Ingredients
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [RequestSizeLimit(104857600)] // 100 MB
        public async Task<ActionResult<ingredients>> PostIngredient([FromForm] ingredientDTO ingredientDTO)
        {
            //把Photo抓出來處理
            var photo = ingredientDTO.photo;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { Errors = errors });
            }
            // Custom validation
            if (string.IsNullOrWhiteSpace(ingredientDTO.ingredientName))
            {
                return BadRequest("食材不能為空");
            }

            if (string.IsNullOrWhiteSpace(ingredientDTO.category))
            {
                return BadRequest("類別需被選擇");
            }

            if (string.IsNullOrWhiteSpace(ingredientDTO.ingredientName))
            {
                return BadRequest("食材不能為空");
            }
            if (photo == null)
            {
                return BadRequest("必須上傳一張食材照片");
            }

            // Save photo
            string photoFileName = null;
            if (photo.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/ingredient");
                photoFileName = $"{Guid.NewGuid()}_{Path.GetFileName(photo.FileName).Replace(" ", "_")}";
                string filePath = Path.Combine(uploadsFolder, photoFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }
            }

            // Create new Recipe entity
            var ingredient = new ingredients
            {
                ingredientName = ingredientDTO.ingredientName,
                category = ingredientDTO.category,
                synonym = ingredientDTO.synonym,
                expireDay = ingredientDTO.expireDay,
                unit = ingredientDTO.unit,
                gram = ingredientDTO.gram,
                photo = photoFileName,
            };

            _context.ingredients.Add(ingredient);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetIngredient", new { id = ingredient.ingredientId }, ingredient);
        }

        // DELETE: api/Ingredients/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIngredient(int id)
        {
            var ingredient = await _context.ingredients.FindAsync(id);
            if (ingredient == null)
            {
                return NotFound();
            }

            _context.ingredients.Remove(ingredient);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool IngredientExists(int id)
        {
            return _context.ingredients.Any(e => e.ingredientId == id);
        }

    }
}
