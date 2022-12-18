using ATARK_Backend.Data;

namespace ATARK_Backend.Models.DTO
{
	public class CellInfoDto
	{
		public int CellNumber { get; set; }
		public int StoreId { get; set; }
		public bool IsLocked { get; set; }
		public string? UserId { get; set; }
		public DateTime? LockTime { get; set; }
	}
}
