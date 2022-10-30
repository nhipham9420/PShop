using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PShop.Models;
using PShop.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PShop.Data
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _dbcontext;

        public DbInitializer(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext dbcontext)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _dbcontext = dbcontext;
        }


        public void Initialize()
        {
            //migrations if they are not applied
            try
            {
                if (_dbcontext.Database.GetPendingMigrations().Count() > 0)
                {
                    _dbcontext.Database.Migrate();
                }
            }
            catch (Exception ex)
            {

            }

            //create roles if they are not created
            if (!_roleManager.RoleExistsAsync(ValueStore.RoleAdmin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(ValueStore.RoleAdmin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(ValueStore.RoleManager)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(ValueStore.RoleCustomer)).GetAwaiter().GetResult();

                //if roles are not created, then we will create admin user as well
                _userManager.CreateAsync(new AppUser
                {
                    UserName = "admin",
                    Email = "admin@test.com",
                    Name = "Admin",
                    PhoneNumber = "0987654321",
                    Address = "test"
                }, "123123").GetAwaiter().GetResult();

                AppUser userAdmin = _dbcontext.AppUsers.FirstOrDefault(u => u.Email == "admin@test.com");
                _userManager.AddToRoleAsync(userAdmin, ValueStore.RoleAdmin).GetAwaiter().GetResult();


                _userManager.CreateAsync(new AppUser
                {
                    UserName = "manager",
                    Email = "manager@test.com",
                    Name = "Manager",
                    PhoneNumber = "0987654321",
                    Address = "test"
                }, "123123").GetAwaiter().GetResult();                

                AppUser userManager = _dbcontext.AppUsers.FirstOrDefault(u => u.Email == "manager@test.com");
                _userManager.AddToRoleAsync(userManager, ValueStore.RoleManager).GetAwaiter().GetResult();


                _userManager.CreateAsync(new AppUser
                {
                    UserName = "customer",
                    Email = "customer@test.com",
                    Name = "Customer",
                    PhoneNumber = "0987654321",
                    Address = "test"
                }, "123123").GetAwaiter().GetResult();                

                AppUser userCustomer = _dbcontext.AppUsers.FirstOrDefault(u => u.Email == "customer@test.com");
                _userManager.AddToRoleAsync(userCustomer, ValueStore.RoleCustomer).GetAwaiter().GetResult();
            }
            return;
        }
    }
}
