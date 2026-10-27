namespace MidtermExam.Models;
public enum SlotType { PrimaryCampaign = 1, PalateCleanser = 2, EndlessSquad = 3 }
public static class SlotTypeExtensions
{
    public static string ToDisplayName(this SlotType slot) => slot switch
    {
        SlotType.PrimaryCampaign => "Main campaign", SlotType.PalateCleanser => "Quick play",
        SlotType.EndlessSquad => "Co-op / endless", _ => "Active slot"
    };
    public static string ToSubtitle(this SlotType slot) => slot switch
    {
        SlotType.PrimaryCampaign => "A story to come back to when you have time to focus.",
        SlotType.PalateCleanser => "A short, familiar game for a quick break.",
        SlotType.EndlessSquad => "A regular game for friends or another round.", _ => "Your rotation"
    };
    public static string DefaultPrimaryLabel(this SlotType slot) => slot switch
    {
        SlotType.PrimaryCampaign => "Progress", SlotType.PalateCleanser => "Typical session",
        SlotType.EndlessSquad => "Current objective", _ => "Progress"
    };
    public static string DefaultSecondaryLabel(this SlotType slot) => slot switch
    {
        SlotType.PrimaryCampaign => "Time played", SlotType.PalateCleanser => "Sessions played",
        SlotType.EndlessSquad => "Sessions played", _ => "Time played"
    };
}
public enum GameStatus { Active = 1, Cleared = 2, Dropped = 3 }
public enum BadgeTier { Bronze = 1, Silver = 2, Gold = 3, Platinum = 4 }
