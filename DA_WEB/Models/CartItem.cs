using DA_WEB.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class CartItem
{
    [Key]
    public int Id { get; set; } // <-- Khóa chính

    [Required]
    public string CartId { get; set; } = null!;
    public Cart Cart { get; set; } = null!; // Navigation property

    [Required]
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!; // Navigation property

    [Required]
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
}