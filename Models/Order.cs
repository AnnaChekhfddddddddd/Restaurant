using System.Collections.Generic;
using System;

namespace Restaurant.Models
{
    public class Order
    {
        public int ID { get; set; }

        // Зв'язок з клієнтом (твоя початкова логіка)
        public int ClientID { get; set; }
        public User Client { get; set; }

        public int? WaiterID { get; set; }
        public User Waiter { get; set; }

        public int StatusID { get; set; }
        public Status Status { get; set; }

        public int PlaceID { get; set; }
        public Place Place { get; set; }

        public int? Price { get; set; } // Тут зберігатимемо загальну суму
        public DateTime OrderDate { get; set; }

        // Використовуємо твою назву, щоб контролер не "сварився"
        public ICollection<OrderFood> OrderFoods { get; set; }
    }
}