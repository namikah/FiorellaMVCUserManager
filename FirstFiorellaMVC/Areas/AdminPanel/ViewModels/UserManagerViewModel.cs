using FirstFiorellaMVC.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace FirstFiorellaMVC.Areas.AdminPanel.ViewModels
{
    public class UserManagerViewModel
    {
        public List<User> Users { get; set; }

        public List<IdentityRole> Roles { get; set; }

        public List<IdentityUserRole<string>> UserRoles { get; set; }

    }
}
