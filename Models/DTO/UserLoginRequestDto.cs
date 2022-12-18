using Microsoft.Build.Framework;

namespace ATARK_Backend.Models.DTO
{
	public class UserLoginRequestDto
	{
		[Required]
		public string Email { get; set; }
		[Required]
		public string Password { get; set; }
	}
}
