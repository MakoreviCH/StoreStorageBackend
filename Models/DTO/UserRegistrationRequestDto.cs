using Microsoft.Build.Framework;

namespace ATARK_Backend.Models.DTO
{
	public class UserRegistrationRequestDto
	{
		[Required]
		public string Email { get; set; }
		[Required]
		public string Password { get; set; }
		[Required]
		public string Phone_Number { get; set; }
		public string First_Name { get; set; }
		public string Last_Name { get; set; }
	}
}
