using Microsoft.AspNetCore.Identity;

namespace ATARK_Backend.Data
{
    public class User : IdentityUser
    {
	    public string? First_Name { get; set; }
        public string? Last_Name { get; set; }
        public string PhoneNumber { get; set; }
        public List<Cell> Cells { get; set; }

	}
}
