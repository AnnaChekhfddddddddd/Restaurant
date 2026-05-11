using System.Collections.Generic;

namespace Restaurant.Models
{
    public class Status
    {
        public int ID { get; set; }
        public string Name { get; set; }  
        ICollection<Order> Orders { get; set; }
    }
}
