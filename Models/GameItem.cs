namespace MidtermExam.Models;

public sealed class GameItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public SlotType Slot { get; set; }
    public string Role => Slot.ToDisplayName();
    public string Title { get; set; } = string.Empty;
    public string Cover { get; set; } = "/images/game-placeholder.svg";
    public string AltText { get; set; } = "Game cover";
    public string Purpose { get; set; } = string.Empty;
    public string PrimaryLabel { get; set; } = "Progress";
    public string PrimaryValue { get; set; } = "0% complete";
    public string SecondaryLabel { get; set; } = "Time logged";
    public string SecondaryValue { get; set; } = "0 hours";
    public int TargetSessionMinutes { get; set; } = 90;
    public int MinBrainBattery { get; set; } = 60;
    public GameStatus Status { get; set; } = GameStatus.Active;
    public DateTime DateEquipped { get; set; } = DateTime.UtcNow;
    public DateTime? DateCompleted { get; set; }
    public string? ClearedNotes { get; set; }
    public string? DroppedReason { get; set; }

    public GameItem() { }

    public GameItem(
        SlotType slot,
        string title,
        string cover,
        string altText,
        string purpose,
        string primaryLabel,
        string primaryValue,
        string secondaryLabel,
        string secondaryValue,
        int targetSessionMinutes = 60,
        int minBrainBattery = 50)
    {
        Id = Guid.NewGuid();
        Slot = slot;
        Title = title;
        Cover = cover;
        AltText = altText;
        Purpose = purpose;
        PrimaryLabel = primaryLabel;
        PrimaryValue = primaryValue;
        SecondaryLabel = secondaryLabel;
        SecondaryValue = secondaryValue;
        TargetSessionMinutes = targetSessionMinutes;
        MinBrainBattery = minBrainBattery;
        Status = GameStatus.Active;
        DateEquipped = DateTime.UtcNow;
    }

    public void MarkCleared(string? notes = null)
    {
        Status = GameStatus.Cleared;
        DateCompleted = DateTime.UtcNow;
        ClearedNotes = notes;
    }

    public void MarkDropped(string? reason = null)
    {
        Status = GameStatus.Dropped;
        DateCompleted = DateTime.UtcNow;
        DroppedReason = reason;
    }

    public void UpdateProgress(string primaryValue, string secondaryValue)
    {
        PrimaryValue = primaryValue;
        SecondaryValue = secondaryValue;
    }

    public double CalculateSuitabilityScore(int timeAvailable, int brainBattery)
    {
        // Suitability calculation based on session time and brain battery fit
        double timeDelta = Math.Abs(timeAvailable - TargetSessionMinutes);
        double batteryDelta = Math.Abs(brainBattery - MinBrainBattery);

        // Lower deltas produce higher suitability scores
        double score = 100.0 - (timeDelta * 0.35 + batteryDelta * 0.45);
        return Math.Clamp(score, 10.0, 99.0);
    }
}
