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
    public class UserGroupsController : ControllerBase
    {
        private readonly RecipeCubeContext _context;

        public UserGroupsController(RecipeCubeContext context)
        {
            _context = context;
        }

        // GET: api/UserGroups
        [HttpGet]
        public async Task<ActionResult<IEnumerable<user_groups>>> GetUserGroups()
        {
            return await _context.user_groups.ToListAsync();
        }

        // GET: api/UserGroups/5
        [HttpGet("{id}")]
        public async Task<ActionResult<user_groups>> GetUserGroup(int id)
        {
            var userGroup = await _context.user_groups.FindAsync(id);

            if (userGroup == null)
            {
                return NotFound();
            }

            return userGroup;
        }

        // PUT: api/UserGroups/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserGroup(int id, user_groups userGroup)
        {
            if (id != userGroup.groupId)
            {
                return BadRequest();
            }

            _context.Entry(userGroup).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserGroupExists(id))
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

        // POST: api/UserGroups
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<user_groups>> PostUserGroup(user_groups userGroup)
        {
            _context.user_groups.Add(userGroup);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserGroup", new { id = userGroup.groupId }, userGroup);
        }

        // DELETE: api/UserGroups/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserGroup(int id)
        {
            var userGroup = await _context.user_groups.FindAsync(id);
            if (userGroup == null)
            {
                return NotFound();
            }

            _context.user_groups.Remove(userGroup);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/UserGroups
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("CreateGroup")]
        public async Task<IActionResult> CreateGroupDTO(CreateGroupDTO userGroupAdd)
        {
            if (userGroupAdd == null)
            {
                return BadRequest("userGroup沒資料");
            }
            var existingGroup = await _context.user_groups.SingleOrDefaultAsync(u => u.groupAdmin == userGroupAdd.group_Admin_Id);
            if (existingGroup != null)
            {
                return NotFound(new { Message = "已有群組" });
            }
            Random Number = new Random();
            var GroupInvite = 0;
            int group_Invite = 0;
            Console.WriteLine();
            Console.WriteLine(group_Invite);
            group_Invite = Number.Next(100000000, 999999999);
            var existingGroupInvite = await _context.user_groups.SingleOrDefaultAsync(u => u.groupInvite == group_Invite);
            Console.WriteLine();
            Console.WriteLine(group_Invite);
            while (existingGroupInvite != null)
            {
                GroupInvite = group_Invite;
            }
            Console.WriteLine();
            Console.WriteLine(group_Invite);

            var newGroup = new user_groups
            {
                groupAdmin = userGroupAdd.group_Admin_Id,
                groupName = userGroupAdd.group_name,
                groupInvite = group_Invite,
            };

            _context.user_groups.Add(newGroup);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "新增成功" });
        }

        private bool UserGroupExists(int id)
        {
            return _context.user_groups.Any(e => e.groupId == id);
        }

    }
}
