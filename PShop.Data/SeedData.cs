using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PShop.Data
{
    public class SeedData
    {
        private readonly ModelBuilder modelBuilder;

        public SeedData(ModelBuilder modelBuilder)
        {
            this.modelBuilder = modelBuilder;
        }

        public void Seed()
        {
            //modelBuilder.Entity<IdentityRole>().HasData(
            //    new IdentityRole() { Id = "1", Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin"},
            //    new IdentityRole() { Id = "2", Name = "Manager", ConcurrencyStamp = "2", NormalizedName = "Manager"},
            //    new IdentityRole() { Id = "3", Name = "Customer", ConcurrencyStamp = "3", NormalizedName = "Customer"}
            //);

            //modelBuilder.Entity<IdentityUserRole<string>>().HasData(  
            //    new IdentityUserRole<string>() { RoleId = "1", UserId = "1" }  
            //);  

            modelBuilder.Entity<Category>().HasData(
                   new Category() { Id = 1, Name = "Type 1" },
                   new Category() { Id = 2, Name = "Type 2" }
            );
        }
    }
}
