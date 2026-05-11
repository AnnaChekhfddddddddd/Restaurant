namespace Restaurant.Models
{
    public class OrderFood
    {
        public int ID { get; set; }
        public int OrderID { get; set; }
        public Order Order { get; set; }

        public int FoodID { get; set; }
        public Food Food { get; set; }

        // ДОДАЙ ЦЕЙ РЯДОК, ЯКЩО ЙОГО НЕМАЄ:
        public int Quantity { get; set; }
    }
}