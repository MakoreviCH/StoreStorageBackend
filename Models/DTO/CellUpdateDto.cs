namespace ATARK_Backend.Models.DTO
{
	public class CellUpdateDto
	{
		public bool IsLocked { get; set; }
		public string? UserId { get; set; }
		public DateTime? LockTime { get; set; }
	}
}
