using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ATARK_Backend.Data;
using MQTTnet.Client;
using MQTTnet.Server;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using ATARK_Backend.Models.DTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ATARK_Backend.Controllers
{
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[Route("api/[controller]")]
	[ApiController]
	public class CellsController : ControllerBase
	{
		private readonly BackendContext _context;
		private readonly IMqttController _controller;
		public CellsController(BackendContext context, IMqttController controller)
		{
			_context = context;
			_controller = controller;
		}

		// GET: api/Cells
		[Authorize(Roles = "Admin")]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<CellInfoDto>>> GetCell()
		{ 

			if (_context.Cell == null)
			{
				return NotFound();
			}
			return await _context.Cell.Select(cell => new CellInfoDto()
			{
				CellNumber = cell.CellNumber,
				StoreId = cell.StoreId,
				IsLocked = cell.IsLocked,
				UserId = cell.UserId,
				 LockTime= cell.LockTime
			}).ToListAsync();
		}
		[Authorize(Roles = "Member,Admin,Manager")]
		[HttpGet("{storeId}")]
		public async Task<ActionResult<IEnumerable<CellInfoDto>>> GetStoreCell(int storeId)
		{
			if (_context.Cell == null)
			{
				return NotFound();
			}

			return  await _context.Cell.Where(ccl=>ccl.StoreId==storeId).Select(cell => new CellInfoDto()
			{
				CellNumber = cell.CellNumber,
				StoreId = cell.StoreId,
				IsLocked = cell.IsLocked,
				UserId = cell.UserId,
				LockTime = cell.LockTime
			}).ToListAsync();
		}
		// GET: api/Cells/5
		[Authorize(Roles = "Member,Admin,Manager")]
		[HttpGet("{cellNumber}/{storeId}")]
		public async Task<ActionResult<CellInfoDto>> GetCell(int cellNumber, int storeId)
		{
			if (_context.Cell == null)
			{
				return NotFound();
			}
			var cell = await _context.Cell.Where(cell => cell.CellNumber == cellNumber && cell.StoreId== storeId).Select(cell => new CellInfoDto()
			{
				CellNumber = cell.CellNumber,
				StoreId = cell.StoreId,
				IsLocked = cell.IsLocked,
				UserId = cell.UserId,
				LockTime = cell.LockTime
			}).FirstOrDefaultAsync();

			if (cell == null)
			{
				return NotFound();
			}

			return cell;
		}



		// PUT: api/Cells/5
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
		[Authorize(Roles = "Member,Admin,Manager")]
		[HttpPut("{cellNumber}/{storeId}")]
		public async Task<IActionResult> PutCell(int cellNumber, int storeId, CellUpdateDto cell)
		{

			var foundCell = await _context.Cell.FindAsync(cellNumber, storeId);
			if (foundCell == null)
			{
				return BadRequest();
			}
			

			foundCell.IsLocked = cell.IsLocked;
			foundCell.UserId = cell.UserId;
			foundCell.LockTime = cell.LockTime;

			try
			{
				await _context.SaveChangesAsync();
				await _controller.PublishMethodAsync("store/" + foundCell.StoreId, _context.GetMessage(storeId));
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!CellExists(cellNumber, storeId))
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


		[Authorize(Roles = "Admin")]
		[HttpPost("{storeId}")]
		public async Task<ActionResult<IEnumerable<CellInfoDto>>> PostCellStore(int storeId)
		{
			
			for (int i = 1; i <= 64; i++)
			{
				Cell cell = new Cell
				{
					CellNumber = i,
					IsLocked = false,
					StoreId = storeId,
					LockTime = null,
					UserId = null
				};
				_context.Cell.Add(cell);
			}

			try
			{
				await _context.SaveChangesAsync();
				await _controller.PublishMethodAsync("store/" + storeId, _context.GetMessage(storeId));
			}
			catch (DbUpdateException)
			{
				if (StoreCellExists(storeId))
				{
					return Conflict();
				}
				else
				{
					throw;
				}
			}

		//	return CreatedAtAction("GetStoreCell", new{ storeId},new object());
		return await GetStoreCell(storeId);
		}

		// POST: api/Cells
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
		[Authorize(Roles = "Admin")]
		[HttpPost]
		public async Task<ActionResult<CellInfoDto>> PostCell(Cell cell)
		{
			if (_context.Cell == null)
			{
				return Problem("Entity set 'BackendContext.Cell'  is null.");
			}
			_context.Cell.Add(cell);
			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException)
			{
				if (CellExists(cell.CellNumber, cell.StoreId))
				{
					return Conflict();
				}
				else
				{
					throw;
				}
			}

			return CreatedAtAction("GetCell", new { cellNumber = cell.CellNumber, storeId = cell.StoreId }, cell);
		}

		// DELETE: api/Cells/5
		[Authorize(Roles = "Admin")]
		[HttpDelete("{cellNumber}/{storeId}")]
		public async Task<IActionResult> DeleteCell(int cellNumber, int storeId)
		{
			if (_context.Cell == null)
			{
				return NotFound();
			}
			var cell = await _context.Cell.FindAsync(cellNumber, storeId);
			if (cell == null)
			{
				return NotFound();
			}

			_context.Cell.Remove(cell);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool CellExists(int cellNumber, int storeId)
		{
			return (_context.Cell?.Any(e => e.CellNumber == cellNumber && e.StoreId == storeId)).GetValueOrDefault();
		}
		private bool StoreCellExists(int storeId)
		{
			return (_context.Cell?.Any(e => e.StoreId == storeId)).GetValueOrDefault();
		}
	}
}
