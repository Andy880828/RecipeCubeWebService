using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeCubeWebService.DTO;
using RecipeCubeWebService.Models;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;

namespace RecipeCubeWebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponsController : ControllerBase
    {
        private readonly RecipeCubeContext _context;

        public CouponsController(RecipeCubeContext context)
        {
            _context = context;
        }

        // 獲取所有優惠券配user_coupons
        // GET: api/Coupons
        [HttpGet("GetCouponsWithUserCoupons")]
        public async Task<ActionResult<IEnumerable<couponsDTO>>> GetCouponsWithUserCoupons()
        {
            var coupons = await (
                from coupon in _context.coupons
                join userCoupon in _context.user_coupons
                    on coupon.couponId equals userCoupon.couponId
                select new couponsDTO
                {
                    couponId = coupon.couponId,
                    couponName = coupon.couponName,
                    couponStatus = coupon.status,
                    discountType = coupon.discountType,
                    discountValue = coupon.discountValue,
                    userConponId = userCoupon.userCouponId,
                    userId = userCoupon.userId,
                    usedStatus = userCoupon.status,
                    acquireDate = userCoupon.acquireDate,
                    minSpend = coupon.minSpend,
                }
                ).ToListAsync();
            return Ok(coupons);
        }


        // GET: api/Coupons
        [HttpGet]
        public async Task<ActionResult<IEnumerable<coupons>>> GetCoupons()
        {
            return await _context.coupons.ToListAsync();
        }

        // GET: api/Coupons/5
        [HttpGet("{id}")]
        public async Task<ActionResult<coupons>> GetCoupon(int id)
        {
            var coupon = await _context.coupons.FindAsync(id);

            if (coupon == null)
            {
                return NotFound();
            }

            return coupon;
        }

        // PUT: api/Coupons/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCoupon(int id, coupons coupon)
        {
            if (id != coupon.couponId)
            {
                return BadRequest();
            }

            _context.Entry(coupon).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CouponExists(id))
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

        // POST: api/Coupons
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<coupons>> PostCoupon(coupons coupon)
        {
            _context.coupons.Add(coupon);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCoupon", new { id = coupon.couponId }, coupon);
        }

        // DELETE: api/Coupons/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCoupon(int id)
        {
            var coupon = await _context.coupons.FindAsync(id);
            if (coupon == null)
            {
                return NotFound();
            }

            _context.coupons.Remove(coupon);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CouponExists(int id)
        {
            return _context.coupons.Any(e => e.couponId == id);
        }

    }
}
