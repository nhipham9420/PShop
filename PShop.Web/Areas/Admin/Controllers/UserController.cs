using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PShop.Data;
using PShop.Models;
using PShop.Models.ViewModels;
using PShop.Utility;

namespace PShopWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ValueStore.RoleAdmin)]
    public class UserController : Controller
    {
        private readonly AppDbContext _dbcontext;
        private readonly UserManager<AppUser> _userManager;

        public UserController(AppDbContext db, UserManager<AppUser> userManager)
        {
            _dbcontext = db;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var model = _dbcontext.AppUsers.Where(u=>u.UserName!=null).ToList();
            var userRole = _dbcontext.UserRoles.ToList();
            var roles = _dbcontext.Roles.ToList();

            //set user to none to not make ui look terrible
            foreach (var user in model)
            {
                var role = userRole.FirstOrDefault(u => u.UserId == user.Id);
                if (role == null)
                {
                    user.Role = "None";
                }
                else
                {
                    user.Role = roles.FirstOrDefault(u => u.Id == role.RoleId).Name;
                }
            }

            return View(model);

        }

        public IActionResult Edit(string id)
        {

            var model = _dbcontext.AppUsers.FirstOrDefault(u => u.Id == id);
            if (model == null)
            {
                return NotFound();
            }
            var userRole = _dbcontext.UserRoles.ToList();
            var roles = _dbcontext.Roles.ToList();
            var role = userRole.FirstOrDefault(u => u.UserId == model.Id);
            if (role != null)
            {
                model.RoleId = roles.FirstOrDefault(u => u.Id == role.RoleId).Id;
            }
            model.RoleList = _dbcontext.Roles.Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id
            });
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppUser user)
        {
            user.RoleList = _dbcontext.Roles.Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id
            });

            if (ModelState.IsValid)
            {
                var userDbValue = _dbcontext.AppUsers.FirstOrDefault(u => u.Id == user.Id);
                userDbValue.Name = user.Name;
                userDbValue.PhoneNumber = user.PhoneNumber;
                userDbValue.Address = user.Address;

                if (userDbValue == null)
                {
                    return NotFound();
                }
                var userRole = _dbcontext.UserRoles.FirstOrDefault(u => u.UserId == userDbValue.Id);
                if (userRole != null)
                {
                    var previousRoleName = _dbcontext.Roles.Where(u => u.Id == userRole.RoleId).Select(e => e.Name).FirstOrDefault();
                    await _userManager.RemoveFromRoleAsync(userDbValue, previousRoleName);

                }

                await _userManager.AddToRoleAsync(userDbValue, _dbcontext.Roles.FirstOrDefault(u => u.Id == user.RoleId).Name);
                _dbcontext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(AppUser user)
        {
            
            if (ModelState.IsValid)
            {
                var userDb = await _userManager.FindByEmailAsync(user.Email);
                //if(userDb == null)
                //{
                //    ModelState.AddModelError("Email", "User not found");
                //    return View();
                //}
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(userDb, resetToken, user.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            var userReturn = _dbcontext.AppUsers.FirstOrDefault(u => u.Id == user.Id);
            var userRole = _dbcontext.UserRoles.ToList();
            var roles = _dbcontext.Roles.ToList();
            var role = userRole.FirstOrDefault(u => u.UserId == userReturn.Id);
            if (role != null)
            {
                userReturn.RoleId = roles.FirstOrDefault(u => u.Id == role.RoleId).Id;
            }
            userReturn.RoleList = _dbcontext.Roles.Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id
            });

            return View(nameof(Edit), userReturn);
        }


        [HttpPost]
        public IActionResult Delete(string id)
        {
            var user = _dbcontext.AppUsers.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            _dbcontext.AppUsers.Remove(user);
            _dbcontext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
