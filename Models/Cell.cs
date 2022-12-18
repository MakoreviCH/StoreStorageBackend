using Microsoft.EntityFrameworkCore;

namespace ATARK_Backend.Data
{
    [PrimaryKey(nameof(CellNumber), nameof(StoreId))]
    public class Cell
    {
        public int CellNumber { get; set; }
        public int StoreId { get; set; }
        public Store Store { get; set; }
        public bool IsLocked { get; set; }
        public string? UserId { get; set; }
        public User User { get; set; }
        public DateTime? LockTime { get; set; }

    }
}
