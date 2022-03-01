using FirstFiorellaMVC.DataAccessLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FirstFiorellaMVC.ViewComponents
{
    public class RoleViewComponent : ViewComponent
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleViewComponent(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string roleName)
        {
            ViewBag.SelectedRole = roleName;
            var roles = await _roleManager.Roles.ToListAsync();

            return View(roles);
        }
    }
}
