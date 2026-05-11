using System.Collections.Generic;

namespace Restaurant.Models
{
    public class User
    {
        public int ID { get; set; } 
        public string Login { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name => $"{FirstName} {LastName}";
        public string Phone { get; set; }
        public string Avatar { get; set; }
        public int RoleID { get; set; }
        public Role Role { get; set; }
        public string FullName { get => FirstName + " " + LastName; }
        public ICollection<Order> Orders { get; set; }

    }
}
