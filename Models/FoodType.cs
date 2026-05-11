using System.Collections.Generic;

namespace Restaurant.Models
{
    public class FoodType
    {
        public int ID { get; set; }
        public string Name { get; set; }
        ICollection<Food> Foods{ get; set; }
    }
}
