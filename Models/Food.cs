using System.Collections.Generic;

namespace Restaurant.Models
{
    public class Food
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int FoodTypeID { get; set; }
        public FoodType FoodType { get; set; }
        public int Price { get; set; }
        public string Image { get; set; }
        public bool IsAvailable { get; set; } = true;
        public ICollection<OrderFood> OrderFoods { get; set; }
    }
}
