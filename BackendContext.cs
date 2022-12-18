using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ATARK_Backend.Controllers;
using ATARK_Backend.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ATARK_Backend
{
    public class BackendContext : IdentityDbContext<User>
    {
        public BackendContext(DbContextOptions<BackendContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
	        base.OnModelCreating(modelBuilder);
	        modelBuilder.Entity<User>()
		        .ToTable("User");

	        modelBuilder.Entity<IdentityRole>(entity =>
	        {
		        entity.ToTable(name: "Role");
	        });
	        modelBuilder.Entity<IdentityUserRole<string>>(entity =>
	        {
		        entity.ToTable("UserRoles");
		        //in case you chagned the TKey type
		        //  entity.HasKey(key => new { key.UserId, key.RoleId });
	        });
		}
        public DbSet<User> User { get; set; } = default!;

        public DbSet<Store> Store { get; set; } = default!;

        public DbSet<Cell> Cell { get; set; } = default!;


        public string GetMessage(int storeId)
        {
            var closedCells = Cell
                .Where(cll => cll.IsLocked && cll.StoreId == storeId)
                .Select(cll => new
                {
                    cll.CellNumber
                });
            int[] cellDecimalArray = new int[8];
            foreach (var obj in closedCells)
            {
                cellDecimalArray[(obj.CellNumber - 1) / 8] += Convert.ToInt32(Math.Pow(2, (obj.CellNumber - 1) % 8));
            }

            string iotData = string.Join(';', cellDecimalArray);

            return iotData;
        }
    }
}
