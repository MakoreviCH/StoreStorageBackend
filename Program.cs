using System.Text;
using ATARK_Backend.Config;
using ATARK_Backend.Controllers;
using ATARK_Backend.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ATARK_Backend.Models;

namespace ATARK_Backend
{
    public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			builder.Services.AddDbContext<BackendContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("BackendContext") ?? throw new InvalidOperationException("Connection string 'BackendContext' not found.")));
			builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));

			builder.Services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
				.AddJwtBearer(jwt =>
				{
					var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JwtConfig:Secret").Value);

					jwt.SaveToken = true;
					jwt.TokenValidationParameters = new TokenValidationParameters()
					{
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(key),
						ValidateIssuer = false, // for dev
						ValidateAudience = false, // for dev
						RequireExpirationTime = false, // for dev -- need to be updated when refresh token is added
						ValidateLifetime = true

					};
				});


			builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = false)
				.AddRoles<IdentityRole>()
				.AddEntityFrameworkStores<BackendContext>();

			// Add services to the container.
			builder.Services.AddScoped<IMqttController, MqttController>();
			builder.Services.AddControllers();
			builder.Services.AddControllers().AddNewtonsoftJson(x =>
				x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
			//builder.Services.
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			using (var scope = app.Services.CreateScope())
			{
				var services = scope.ServiceProvider;
				var userManager = services.GetRequiredService<UserManager<User>>();
				var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
				await ContextSeed.SeedRolesAsync(userManager, roleManager);
				var poweruser = new User
				{

					UserName = builder.Configuration["AppSettings:AdminUserEmail"],
					Email = builder.Configuration["AppSettings:AdminUserEmail"],
					PhoneNumber = builder.Configuration["AppSettings:AdminUserPhone"],
				};
				//Ensure you have these values in your appsettings.json file
				string userPWD = builder.Configuration["AppSettings:AdminUserPassword"];
				var _user = await userManager.FindByEmailAsync(builder.Configuration["AppSettings:AdminUserEmail"]);

				if (_user == null)
				{
					var createPowerUser = await userManager.CreateAsync(poweruser, userPWD);
					if (createPowerUser.Succeeded)
					{
						//here we tie the new user to the role
						await userManager.AddToRoleAsync(poweruser, "Admin");

					}
				}
			}


			var mqqtController =
				new MqttController(app.Services.CreateScope().ServiceProvider.GetService<BackendContext>() ?? throw new InvalidOperationException());
			await mqqtController.Handle_Received_Application_Message();
			app.UseHttpsRedirection();
			app.UseAuthentication();
			app.UseAuthorization();
			app.MapControllers();

			app.Run();
		}
		/*
				private static async Task CreateRoles(BackendContext context)
				{
					//initializing custom roles 
					var roleStore = new RoleStore<IdentityRole>(context); //Pass the instance of your DbContext here
					var roleManager = new RoleManager<IdentityRole>(roleStore);
					string[] roleNames = { "Admin", "Manager", "Member" };
					IdentityResult roleResult;

					foreach (var roleName in roleNames)
					{
						var roleExist = await RoleManager.RoleExistsAsync(roleName);
						if (!roleExist)
						{
							//create the roles and seed them to the database: Question 1
							roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
						}
					}

							//Here you could create a super user who will maintain the web app
								var poweruser = new User
								{

									UserName = builder.Configuration["AppSettings:UserName"],
									Email = builder.Configuration["AppSettings:UserEmail"],
								};
								//Ensure you have these values in your appsettings.json file
								string userPWD = builder.Configuration["AppSettings:UserPassword"];
								var _user = await UserManager.FindByEmailAsync(builder.Configuration["AppSettings:AdminUserEmail"]);

								if (_user == null)
								{
									var createPowerUser = await UserManager.CreateAsync(poweruser, userPWD);
									if (createPowerUser.Succeeded)
									{
										//here we tie the new user to the role
										await UserManager.AddToRoleAsync(poweruser, "Admin");

									}
								}
								*/
	}
}
