
namespace CA.WEB.API.Model
{
    public class Stock
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}