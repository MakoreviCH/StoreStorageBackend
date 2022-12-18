using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ATARK_Backend.Data;
using ATARK_Backend.Models.DTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace ATARK_Backend.Controllers
{
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[Route("api/[controller]")]
	[ApiController]
	public class StoresController : ControllerBase
	{
		private readonly BackendContext _context;

		public StoresController(BackendContext context)
		{
			_context = context;
		}


		// GET: api/Stores
		[Authorize(Roles = "Admin")]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Store>>> GetStore()
		{
			if (_context.Store == null)
			{
				return NotFound();
			}
			return await _context.Store.Include(store => store.Cells).ToListAsync();
		}
		[Authorize(Roles = "Member,Admin,Manager")]
		[HttpGet("info")]
		public async Task<ActionResult<List<StoreInfoDto>>> GetStoreInfo()
		{
			if (_context.Store == null)
			{
				return NotFound();
			}

			return await _context.Store.Select(store=> new StoreInfoDto()
			{
				StoreId = store.StoreId,
				Phone = store.PhoneNumber,
				Adress = store.StoreAdress,
				Manager = store.ManagerName
			}).ToListAsync();

		}

		// GET: api/Stores/5
		[Authorize(Roles = "Admin")]
		[HttpGet("{id}")]
		public async Task<ActionResult<Store>> GetStore(int id)
		{
			if (_context.Store == null)
			{
				return NotFound();
			}
			var store = await _context.Store.Include(store => store.Cells).Where(store=> store.StoreId == id).FirstOrDefaultAsync();

			if (store == null)
			{
				return NotFound();
			}

			return store;
		}

		[Authorize(Roles = "Member,Admin,Manager")]
		[HttpGet("info/{id}")]
		public async Task<ActionResult<object>> GetStoreInfo(int id)
		{
			if (_context.Store == null)
			{
				return NotFound();
			}
			var store = await _context.Store.Where(st => st.StoreId == id).Select(store => new StoreInfoDto()
			{
				StoreId = store.StoreId,
				Adress = store.StoreAdress,
				Manager = store.ManagerName,
				Phone = store.PhoneNumber

			}).FirstOrDefaultAsync(); ;

			if (store == null)
			{
				return NotFound();
			}

			return store;
		}

		// PUT: api/Stores/5
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
		[Authorize(Roles = "Admin,Manager")]
		[HttpPut("{id}")]
		public async Task<IActionResult> PutStore(int id, StoreCreateRequestDto store)
		{
			if (_context.Store == null)
			{
				return BadRequest();
			}

			var foundStore = await _context.Store.FindAsync(id);
			if (foundStore == null)
			{
				return NotFound();
			}
			
			foundStore.StoreAdress = store.StoreAdress;
			foundStore.ManagerName = store.ManagerName;
			foundStore.PhoneNumber = store.PhoneNumber;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!StoreExists(id))
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

		// POST: api/Stores
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
		[Authorize(Roles = "Admin")]
		[HttpPost]
		public async Task<ActionResult<Store>> PostStore(StoreCreateRequestDto store)
		{
			if (_context.Store == null)
			{
				return Problem("Entity set 'BackendContext.Store'  is null.");
			}
			Store newStore = new Store()
			{
				StoreAdress = store.StoreAdress,
				PhoneNumber = store.PhoneNumber,
				ManagerName = store.ManagerName
			};
			_context.Store.Add(newStore);
			await _context.SaveChangesAsync();

			return CreatedAtAction("GetStore", new { id = newStore.StoreId });
		}

		// DELETE: api/Stores/5
		[Authorize(Roles = "Admin")]
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteStore(int id)
		{
			if (_context.Store == null)
			{
				return NotFound();
			}
			var store = await _context.Store.FindAsync(id);
			if (store == null)
			{
				return NotFound();
			}

			_context.Store.Remove(store);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool StoreExists(int id)
		{
			return (_context.Store?.Any(e => e.StoreId == id)).GetValueOrDefault();
		}
	}
}
