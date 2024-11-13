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
    public class couponsController : ControllerBase
    {
        private readonly RecipeCubeContext _context;

        public couponsController(RecipeCubeContext context)
        {
            _context = context;
        }

        // GET: api/coupons
        [HttpGet]
        public async Task<ActionResult<IEnumerable<coupons>>> Getcoupons()
        {
            return await _context.coupons.ToListAsync();
        }

        // GET: api/coupons/5
        [HttpGet("{id}")]
        public async Task<ActionResult<coupons>> Getcoupons(int id)
        {
            var coupons = await _context.coupons.FindAsync(id);

            if (coupons == null)
            {
                return NotFound();
            }

            return coupons;
        }

        // PUT: api/coupons/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> Putcoupons(int id, coupons coupons)
        {
            if (id != coupons.couponId)
            {
                return BadRequest();
            }

            _context.Entry(coupons).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!couponsExists(id))
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

        // POST: api/coupons
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<coupons>> Postcoupons(coupons coupons)
        {
            _context.coupons.Add(coupons);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Getcoupons", new { id = coupons.couponId }, coupons);
        }

        // DELETE: api/coupons/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletecoupons(int id)
        {
            var coupons = await _context.coupons.FindAsync(id);
            if (coupons == null)
            {
                return NotFound();
            }

            _context.coupons.Remove(coupons);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool couponsExists(int id)
        {
            return _context.coupons.Any(e => e.couponId == id);
        }
    }
}
