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
    public class PantryManagementsController : ControllerBase
    {
        private readonly RecipeCubeContext _context;

        public PantryManagementsController(RecipeCubeContext context)
        {
            _context = context;
        }

        // GET: api/PantryManagements
        [HttpGet]
        public async Task<ActionResult<IEnumerable<pantry_management>>> GetPantryManagements()
        {
            return await _context.pantry_management.ToListAsync();
        }

        // GET: api/PantryManagements/5
        [HttpGet("{userId}")]
        public async Task<ActionResult<pantry_management>> GetPantryManagement(string userId)
        {
            var userInfo = await _context.user
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                GroupId = u.groupId,
                UserName = u.UserName
            })
            .FirstOrDefaultAsync();

            var groupId = userInfo?.GroupId;
            var userName = userInfo?.UserName;

            if (groupId == null || userName == null)
            {
                return BadRequest();
            }

            var userPantryManagements = await _context.pantry_management
                .Where(i => i.groupId == groupId)
                .ToListAsync(); //抓該群組id的所有庫存

            var ingredients = await _context.ingredients.ToListAsync();

            List<PantryDTO> pantryDTOs = new List<PantryDTO>();

            foreach (pantry_management userPantryManagement in userPantryManagements)
            {
                var ingredient = ingredients.Where(i => i.ingredientId == userPantryManagement.ingredientId);
                var ownerName = await _context.user.Where(u => u.Id == userPantryManagement.ownerId).Select(u => u.UserName).FirstOrDefaultAsync();
                var pantryUserName = await _context.user.Where(u => u.Id == userPantryManagement.userId).Select(u => u.UserName).FirstOrDefaultAsync();

                PantryDTO pantryDTO = new PantryDTO
                {
                    pantryId = userPantryManagement.pantryId,
                    groupId = (int)groupId,
                    userId = userPantryManagement.userId,
                    userName = pantryUserName,
                    ownerId = userPantryManagement.ownerId,
                    ownerName = ownerName,
                    ingredientId = userPantryManagement.ingredientId,
                    ingredientName = ingredient.Select(i => i.ingredientName).FirstOrDefault(),
                    quantity = userPantryManagement.quantity,
                    unit = ingredient.Select(i => i.unit).FirstOrDefault(),
                    action = userPantryManagement.action,
                    time = userPantryManagement.time,
                };


                pantryDTOs.Add(pantryDTO);
            }

            return Ok(pantryDTOs); // Return the list with an OK response
        }


        // POST: api/PantryManagements
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<pantry_management>> PostPantryManagement(pantry_management pantryManagement)
        {
            _context.pantry_management.Add(pantryManagement);
            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
