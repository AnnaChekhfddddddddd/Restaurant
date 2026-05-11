using System.Collections;
using System.Collections.Generic;

namespace Restaurant.Models
{
    public class Place
    {
        public int ID { get; set; }
        public string Name { get; set; }

        // Нове поле: хто зараз обслуговує цей столик
        public int? WaiterID { get; set; }
        public virtual User? Waiter { get; set; }
    }
}
