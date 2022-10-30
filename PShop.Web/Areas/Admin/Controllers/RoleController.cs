using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PShop.Data;
using PShop.Models;
using PShop.Models.ViewModels;
using PShop.Utility;

namespace PShopWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ValueStore.RoleAdmin)]
    public class RoleController : Controller
    {
        private readonly AppDbContext _dbcontext;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;


        public RoleController(AppDbContext dbcontext, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _dbcontext = dbcontext;
            _roleManager = roleManager;
            _userManager = userManager;
        }


        public IActionResult Index()
        {
            var model = new RoleVM()
            {
                ListRoles = _dbcontext.Roles.ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RoleVM model)
        {
            if (string.IsNullOrEmpty(model.Role.Id))
            {
                //create
                await _roleManager.CreateAsync(new IdentityRole() { Name = model.Role.Name });
            }
            else
            {
                //update
                var roleDb = _dbcontext.Roles.FirstOrDefault(u => u.Id == model.Role.Id);
                roleDb.Name = model.Role.Name;
                roleDb.NormalizedName = model.Role.Name.ToUpper();
                var result = await _roleManager.UpdateAsync(roleDb);
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(string? id)
        {
            var model = new RoleVM()
            {
                Role = _dbcontext.Roles.FirstOrDefault(u => u.Id == id)
            };

            await _roleManager.DeleteAsync(model.Role);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string? id)
        {
            var model = new RoleVM()
            {
                Role = _dbcontext.Roles.FirstOrDefault(u => u.Id == id),
                ListRoles = _dbcontext.Roles.ToList()
            };

            return View("Index", model);
        }
    }
}
