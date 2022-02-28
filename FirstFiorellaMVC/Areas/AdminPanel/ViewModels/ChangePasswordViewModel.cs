using FirstFiorellaMVC.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace FirstFiorellaMVC.Areas.AdminPanel.ViewModels
{
    public class ChangePasswordViewModel : PasswordViewModel
    {
        [Required, DataType(DataType.Password)]
        public string OldPassword { get; set; }
    }
}
