using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations.Schema;

namespace Order.API.Models;

public class Order
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string BuyerId { get; set; }
    public Address Address { get; set; }
    public ICollection<OrderItem> Items { get; set; } = [];

    public OrderStatus Status { get; set; }

    public string FailMessage { get; set; }
}

public enum OrderStatus
{
    Suspend,
    Complete,
    Fail
}

[Owned]
public class Address
{
    public string Line { get; set; }
    public string Province { get; set; }
    public string District { get; set; }
}

public class OrderItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public int OrderId { get; set; }

    public Order Order { get; set; }

    public int Count { get; set; }
}