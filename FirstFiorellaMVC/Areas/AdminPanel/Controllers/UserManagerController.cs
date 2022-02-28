using FirstFiorellaMVC.Areas.AdminPanel.ViewModels;
using FirstFiorellaMVC.Controllers;
using FirstFiorellaMVC.Data;
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
    [Authorize(Roles = RoleConstants.AdminRole)]
    public class UserManagerController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagerController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager = null)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public IActionResult ChangePassword(string id)
        {
            var isUser = _userManager.FindByIdAsync(id);
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
                return RedirectToAction("Index", nameof(ErrorController), BadRequest());

            await _userManager.ChangePasswordAsync(isUser, changePasswordVM.OldPassword, changePasswordVM.Password);

            return RedirectToAction(nameof(Index));
        }
    }
}
