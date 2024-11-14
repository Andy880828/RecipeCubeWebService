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
    public class InventoriesController : ControllerBase
    {
        private readonly RecipeCubeContext _context;

        public InventoriesController(RecipeCubeContext context)
        {
            _context = context;
        }

        // GET: api/Inventories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<inventory>>> GetInventories()
        {
            return await _context.inventory.ToListAsync();
        }

        // GET: api/Inventories/5596af67-3b2a-4d9a-9687-6d290122fd2b
        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<inventoryDTO>>> GetInventory(string userId)
        {
            var userInfo = await _context.user
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                GroupId = u.groupId,
                UserName = u.UserName
            })
            .FirstOrDefaultAsync();

            // 從結果中提取 GroupId 和 UserName
            var groupId = userInfo?.GroupId;
            var userName = userInfo?.UserName;

            if (groupId == null || userName == null)
            {
                return BadRequest();
            }

            var userInventories = await _context.inventory
                .Where(i => i.groupId == groupId)
                .ToListAsync(); //抓該群組id的所有庫存

            var ingredients = await _context.ingredients.ToListAsync(); //抓所有食材的資料

            //為了計算天數要先抓三天後
            DateTime today = DateTime.Now;
            DateTime threeDayAfter = today.AddDays(3);
            List<inventoryDTO> inventoryDTOs = new List<inventoryDTO>();
            foreach (inventory userInventory in userInventories)
            {
                var ingredient = ingredients.Where(i => i.ingredientId == userInventory.ingredientId);
                inventoryDTO inventoryDTO = new inventoryDTO
                {
                    inventoryId = userInventory.inventoryId,
                    groupId = (int)groupId,
                    userId = userInventory.userId,
                    userName = userName,
                    ingredientId = userInventory.ingredientId,
                    quantity = userInventory.quantity,
                    expiryDate = userInventory.expiryDate,
                    isExpiring = userInventory.expiryDate < threeDayAfter && userInventory.expiryDate > today ? true : false,
                    isExpired = userInventory.expiryDate < today ? true : false,
                    visibility = userInventory.visibility,
                    ingredientName = ingredient.Select(i => i.ingredientName).FirstOrDefault(),
                    category = ingredient.Select(i => i.category).FirstOrDefault(),
                    synonym = ingredient.Select(i => i.synonym).FirstOrDefault(),
                    unit = ingredient.Select(i => i.unit).FirstOrDefault(),
                    gram = ingredient.Select(i => i.gram).FirstOrDefault(),
                    photo = ingredient.Select(i => i.photo).FirstOrDefault(),
                };
                inventoryDTOs.Add(inventoryDTO);
            }

            return Ok(inventoryDTOs); // Return the list with an OK response
        }


        // PUT: api/Inventories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInventory(int id, inventory inventory)
        {
            if (id != inventory.inventoryId)
            {
                return BadRequest();
            }

            _context.Entry(inventory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InventoryExists(id))
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

        // POST: api/Inventories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<inventory>> PostInventory(inventory inventory)
        {
            _context.inventory.Add(inventory);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/Inventories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventory(int id)
        {
            var inventory = await _context.inventory.FindAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            _context.inventory.Remove(inventory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InventoryExists(int id)
        {
            return _context.inventory.Any(e => e.inventoryId == id);
        }

    }
}
