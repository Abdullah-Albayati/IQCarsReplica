
namespace Template.Entities.CarEntity;

using Template.Entities.User; 

public class Car
{
    public Guid Id { get; set; }
    public int ListingOwnerId { get; set; }
    public string Name { get; set; } = String.Empty;
    public string Make {get; set;} = String.Empty;
    public string Trim {get; set;} = String.Empty;
    public string Spec {get; set;} = String.Empty;
    public string Color {get; set;} = String.Empty;
    public string Year {get; set;} = String.Empty;
    public string Description {get; set;} = String.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
