namespace Template.DTOs;

public class UserDto
{
	public int Id { get; set; }
	public string Username { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string Role { get; set; } = string.Empty;
	public string? RefreshToken { get; set; }
	public DateTime? RefreshTokenExpiry { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

