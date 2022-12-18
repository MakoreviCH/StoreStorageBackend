using Microsoft.AspNetCore.Identity;
using System;
using ATARK_Backend.Data;

namespace ATARK_Backend.Models
{
	public static class ContextSeed
	{
		static string[] roleNames = { "Admin", "Manager", "Member" };
		public static async Task SeedRolesAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
		{
			//Seed Roles
			foreach (var roleName in roleNames)
			{
				var roleExist = await roleManager.RoleExistsAsync(roleName);
				if (!roleExist)
				{
					//create the roles and seed them to the database: Question 1
					await roleManager.CreateAsync(new IdentityRole(roleName));
				}
			}

			
		}
	}
}
