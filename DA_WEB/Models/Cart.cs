namespace DA_WEB.Models
{
    public class Cart
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); // SessionId hoặc UserId
        public string? UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<CartItem> Items { get; set; } = new();
    }
}