namespace Template.DTOs;

using System.ComponentModel.DataAnnotations;

public class CarListingForm
{
    [Required(ErrorMessage = "Listing name is required")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Phone] // Validates phone number format
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public string Make { get; set; } = string.Empty;

    [Required]
    public string Model { get; set; } = string.Empty; // Added Model (e.g., Civic)

    public string? Trim { get; set; } // Optional
    
    public string? Spec { get; set; } // GCC, US, etc.

    public string? Color { get; set; }

    [Required]
    [Range(1900, 2100)] // Ensures year is a valid number
    public int Year { get; set; }

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    // Nested DTO for Specs
    public CarSpecificationsDto Specifications { get; set; } = new();
}

public class CarSpecificationsDto
{
    public string? EngineSize { get; set; }
    public string? FuelType { get; set; }
    public string? Transmission { get; set; }
}