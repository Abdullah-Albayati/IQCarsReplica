namespace Template.DTOs;

public class CarListingDto
{
    public Guid Id { get; set; }
    public int ListingOwnerId { get; set; }
    public string Name { get; set; } = String.Empty;
    public string Make { get; set; } = String.Empty;
    public string Model { get; set; } = String.Empty;
    public string Year { get; set; } = String.Empty;
    public string Color { get; set; } = String.Empty;
    public string Description { get; set; } = String.Empty;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
