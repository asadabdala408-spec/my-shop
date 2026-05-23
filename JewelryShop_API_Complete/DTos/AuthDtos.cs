using Jewelryshop.Api.Models;

namespace Jewelryshop.Api.DTOs;

public record RegisterRequest(string FullName, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record AuthResponse(int UserId, string FullName, string Email, UserRole Role);
