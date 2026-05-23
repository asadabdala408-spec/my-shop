namespace Jewelryshop.Api.DTOs;

public record CategoryRequest(string Name, string? Description);
public record ProductRequest(string Name, string Description, decimal Price, int StockQuantity, string? ImageUrl, int CategoryId);
public record CartItemRequest(int ProductId, int Quantity);
public record UpdateOrderStatusRequest(string Status);
