using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ATARK_Backend.Data;
using ATARK_Backend.Models.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ATARK_Backend.Controllers
{
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly BackendContext _context;

        public UsersController(BackendContext context)
        {
			_context = context;
		}

		// GET: api/Users
		[Authorize(Roles = "Admin")]
		[HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUser()
        {
          if (_context.User == null)
          {
              return NotFound();
          }
            return await _context.User.Include(user => user.Cells).ToListAsync();
        }

		// GET: api/Users/5
		[Authorize(Roles = "Admin")]
		[HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(string id)
        {
          if (_context.User == null)
          {
              return NotFound();
          }	
          var user = await _context.User.Include(user => user.Cells).Where(user => user.Id == id).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }


        [Authorize(Roles = "Member,Admin,Manager")]
		[HttpGet("userInfo/{id}")]
		public async Task<ActionResult<object>> GetUserInfo(string id)
        {
	        if (_context.User == null)
	        {
		        return NotFound();
	        }
	        var user = await _context.User.Where(us => us.Id == id).Select(identityUser => new
	        {
		        UserId = identityUser.Id,
                Email = identityUser.Email,
		        FirstName = identityUser.First_Name,
		        LastName = identityUser.Last_Name,
		        Phone = identityUser.PhoneNumber
	        }).FirstOrDefaultAsync();

	        if (user == null)
	        {
		        return NotFound();
	        }

	        return user;
        }

		// PUT: api/Users/5
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
		[Authorize(Roles = "Member,Admin,Manager")]
		[HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, UserEditRequestDto user)
        {
	        if (!ModelState.IsValid)
	        {
				return BadRequest();
				

			}

	        var foundUser = await _context.User.FindAsync(id);
	        if (foundUser == null)
	        {
		        return BadRequest();
	        }

	        foundUser.PhoneNumber = user.Phone_Number;
            foundUser.First_Name = user.First_Name;
            foundUser.Last_Name = user.Last_Name;

	        try
	        {
		        await _context.SaveChangesAsync();
	        }
	        catch (DbUpdateConcurrencyException)
	        {
		        if (!UserExists(id))
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

		// POST: api/Users
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
		/*
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
          if (_context.User == null)
          {
              return Problem("Entity set 'BackendContext.User' is null.");
          }
            _context.User.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }
        [HttpPost("register")]
		public async Task<ActionResult<User>> RegisterUser(User user)
        {
	        if (_context.User == null)
	        {
		        return Problem("Entity set 'BackendContext.User' is null.");
	        }
	        if (!_context.User.Any(us => us.Login == user.Login))
	        {
				_context.User.Add(user);
				await _context.SaveChangesAsync();
			}
	        else
	        {
		        return Conflict();
	        }
	        return CreatedAtAction("GetUser", new { id = user.UserId }, user);
        }
*/
		// DELETE: api/Users/5
		[Authorize(Roles = "Member,Admin,Manager")]
		[HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (_context.User == null)
            {
                return NotFound();
            }
            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.User.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(string id)
        {
            return (_context.User?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
