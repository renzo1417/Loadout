namespace MidtermExam.Models;

public sealed class Badge
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "🎖️";
    public BadgeTier Tier { get; set; } = BadgeTier.Bronze;
    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

    public Badge() { }

    public Badge(string id, string name, string description, string icon, BadgeTier tier)
    {
        Id = id;
        Name = name;
        Description = description;
        Icon = icon;
        Tier = tier;
        UnlockedAt = DateTime.UtcNow;
    }
}
