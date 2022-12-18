namespace ATARK_Backend.Data
{
    public class Store
    {
        public int StoreId { get; set; }
        public string StoreAdress { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ManagerName { get; set; }
        public List<Cell> Cells { get; set; }
	}
}
