using System.ComponentModel.DataAnnotations;

namespace Restaurant.Models
{
    public class RegisterModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }

        [Required(ErrorMessage = "Не вказан логін")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Не вказан пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Паролі не збігаються")]
        public string ConfirmPassword { get; set; }

        public int RoleID { get; set; }
        public string Avatar { get; set; }
    }
}