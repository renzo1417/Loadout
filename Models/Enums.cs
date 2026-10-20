namespace MidtermExam.Models;

public enum SlotType
{
    PrimaryCampaign = 1,
    PalateCleanser = 2,
    EndlessSquad = 3
}

public static class SlotTypeExtensions
{
    public static string ToDisplayName(this SlotType slot) => slot switch
    {
        SlotType.PrimaryCampaign => "Primary campaign",
        SlotType.PalateCleanser => "Palate cleanser",
        SlotType.EndlessSquad => "Endless or squad",
        _ => "Active slot"
    };

    public static string ToSubtitle(this SlotType slot) => slot switch
    {
        SlotType.PrimaryCampaign => "The Heavy Hitter · Focus & story commitment",
        SlotType.PalateCleanser => "The Chill Game · Low-stress, short session",
        SlotType.EndlessSquad => "The Multiplayer Rotation · Co-op & competitive",
        _ => "Tactical rotation"
    };

    public static string DefaultPrimaryLabel(this SlotType slot) => slot switch
    {
        SlotType.PrimaryCampaign => "Progress",
        SlotType.PalateCleanser => "Average run",
        SlotType.EndlessSquad => "Squad status",
        _ => "Metric 1"
    };

    public static string DefaultSecondaryLabel(this SlotType slot) => slot switch
    {
        SlotType.PrimaryCampaign => "Time logged",
        SlotType.PalateCleanser => "Runs finished",
        SlotType.EndlessSquad => "Weekly objective",
        _ => "Metric 2"
    };
}

public enum GameStatus
{
    Active = 1,
    Cleared = 2,
    Dropped = 3
}

public enum BadgeTier
{
    Bronze = 1,
    Silver = 2,
    Gold = 3,
    Platinum = 4
}
