using FirstFiorellaMVC.Areas.AdminPanel.ViewModels;
using FirstFiorellaMVC.Controllers;
using FirstFiorellaMVC.Data;
using FirstFiorellaMVC.DataAccessLayer;
using FirstFiorellaMVC.Models;
using FirstFiorellaMVC.ViewModels;
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
        private readonly AppDbContext _dbContext;

        public UserManagerController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager = null, AppDbContext dbContext = null)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
        }

        [AllowAnonymous]
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

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var isOldPassword = await _userManager.CheckPasswordAsync(user, changePasswordVM.OldPassword);
            if (!isOldPassword)
                return BadRequest();

            var result = await _userManager.ChangePasswordAsync(user, changePasswordVM.OldPassword, changePasswordVM.Password);
            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                    return View();  
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ChangeRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
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

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var oldRoles = await _userManager.GetRolesAsync(user);
            foreach (var item in oldRoles)
            {
                await _userManager.RemoveFromRoleAsync(user, item);
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

        public async Task<IActionResult> ChangeStatus(string id, bool status)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            if (status)
                user.Status = false;
            else
                user.Status = true;

            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult AddUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(registerViewModel);
            }

            var existUser = await _userManager.FindByNameAsync(registerViewModel.Username);
            if (existUser != null)
            {
                ModelState.AddModelError("Username", "Allready exist username");
                return View(registerViewModel);
            }

            var user = new User()
            {
                FullName = registerViewModel.Fullname,
                UserName = registerViewModel.Username,
                Email = registerViewModel.Email,
                Status = false
            };

            var result = await _userManager.CreateAsync(user, registerViewModel.Password);
            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
                return View(registerViewModel);
            }

            result = await _userManager.AddToRoleAsync(user, RoleConstants.UserRole);
            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
                return View(registerViewModel);
            }

            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
                return View(registerViewModel);
            }

            return RedirectToAction("Index", "UserManager");
        }
    }
}
