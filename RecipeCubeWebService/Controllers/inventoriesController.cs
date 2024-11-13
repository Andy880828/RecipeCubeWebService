using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeCubeWebService.Models;

namespace RecipeCubeWebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class inventoriesController : ControllerBase
    {
        private readonly RecipeCubeContext _context;

        public inventoriesController(RecipeCubeContext context)
        {
            _context = context;
        }

        // GET: api/inventories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<inventory>>> Getinventory()
        {
            return await _context.inventory.ToListAsync();
        }

        // GET: api/inventories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<inventory>> Getinventory(int id)
        {
            var inventory = await _context.inventory.FindAsync(id);

            if (inventory == null)
            {
                return NotFound();
            }

            return inventory;
        }

        // PUT: api/inventories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> Putinventory(int id, inventory inventory)
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
                if (!inventoryExists(id))
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

        // POST: api/inventories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<inventory>> Postinventory(inventory inventory)
        {
            _context.inventory.Add(inventory);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Getinventory", new { id = inventory.inventoryId }, inventory);
        }

        // DELETE: api/inventories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deleteinventory(int id)
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

        private bool inventoryExists(int id)
        {
            return _context.inventory.Any(e => e.inventoryId == id);
        }
    }
}
