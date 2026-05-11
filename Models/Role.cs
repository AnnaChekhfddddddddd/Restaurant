using System.Collections;
using System.Collections.Generic;

namespace Restaurant.Models
{
    public class Role
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public virtual ICollection<User> Users { get; set; } // Додай public!
    }

}

