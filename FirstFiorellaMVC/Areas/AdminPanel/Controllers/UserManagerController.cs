using FirstFiorellaMVC.Areas.AdminPanel.ViewModels;
using FirstFiorellaMVC.Controllers;
using FirstFiorellaMVC.Data;
using FirstFiorellaMVC.DataAccessLayer;
using FirstFiorellaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FirstFiorellaMVC.Areas.AdminPanel.Controllers
{
    [Area("AdminPanel")]
    //[Authorize(Roles = RoleConstants.AdminRole)]
    public class UserManagerController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _dbContext;

        public UserManagerController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager = null, AppDbContext dbContext = null)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var roles = await _roleManager.Roles.ToListAsync();
            var UserRoles = await _dbContext.UserRoles.ToListAsync();

            return View(new UserManagerViewModel()
            {
                Users = users,
                Roles = roles,
                UserRoles = UserRoles
            });
        }

        public async Task<IActionResult> ChangePassword(string id)
        {
            var isUser = await _userManager.FindByIdAsync(id);
            if (isUser == null)
                return NotFound();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string id, ChangePasswordViewModel changePasswordVM)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Incorrect");
                return View(changePasswordVM);
            }

            var isUser = await _userManager.FindByIdAsync(id);
            if (isUser == null)
                return NotFound();

            var isOldPassword = await _userManager.CheckPasswordAsync(isUser, changePasswordVM.OldPassword);
            if (!isOldPassword)
                return BadRequest();

            await _userManager.ChangePasswordAsync(isUser, changePasswordVM.OldPassword, changePasswordVM.Password);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ChangeRole(string id)
        {
            var isUser = await _userManager.FindByIdAsync(id);
            if (isUser == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(isUser);
            ViewBag.CurrentRoleName = roles.FirstOrDefault();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string id, RoleManagerViewModel roleManagerVM)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Incorrect");
                return View(roleManagerVM);
            }

            var isUser = await _userManager.FindByIdAsync(id);
            if (isUser == null)
                return NotFound();

            var oldRoles = await _userManager.GetRolesAsync(isUser);
            foreach (var item in oldRoles)
            {
                await _userManager.RemoveFromRoleAsync(isUser, item);
            }

            var newRole = _roleManager.Roles.FirstOrDefault(x=>x.Id == roleManagerVM.RoleId);
            if (newRole == null)
            {
                ModelState.AddModelError("", "Please choose correct role");
                return View(roleManagerVM);
            }

            var result = await _userManager.AddToRoleAsync(isUser, newRole.Name);
            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
                return View(roleManagerVM);
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
