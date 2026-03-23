
namespace Template.Entities.CarEntity;

using Template.Entities.User; 

public class CarListing
{
    public Guid Id { get; set; }
    public int ListingOwnerId { get; set; }
    public string Name { get; set; } = String.Empty;
    public string PhoneNumber { get; set; } = String.Empty;
    public string Make {get; set;} = String.Empty;
    public string Trim {get; set;} = String.Empty;
    public string Spec {get; set;} = String.Empty;
    public string Color {get; set;} = String.Empty;
    public string Year {get; set;} = String.Empty;
    public string Description {get; set;} = String.Empty;
    public CarSpecifications Specifications { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class CarSpecifications
{
    public string? EngineSize { get; set; }
    public string? Cylinders { get; set; }
    public string? Transmission { get; set; }
    public string? FuelType  { get; set; }
    
}