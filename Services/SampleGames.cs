using MidtermExam.Models;
namespace MidtermExam.Services;

public static class SampleGames
{
    // Example progress is only demo data; covers are supplied by the project owner.
    public static GameItem[] Create() => [CallOfDuty(), Roblox(), Peak()];

    public static GameItem[] Catalog() => [CallOfDuty(), Peak(), RvThereYet(), Roblox(), Valorant()];

    public static IEnumerable<GameItem> ForRole(SlotType role) => Catalog().Where(game => game.Slot == role);

    private static GameItem CallOfDuty() => new(SlotType.PrimaryCampaign,
        "Call of Duty", "/images/call-of-duty.png", "Call of Duty cover showing three soldiers",
        "Make time for the next mission in your campaign.", "Progress", "Mission 4", "Time played", "3 hours", 90, 65);

    private static GameItem Roblox() => new(SlotType.PalateCleanser,
        "Roblox", "/images/roblox.png", "Roblox logo on a blue background",
        "Pick a familiar experience for a short break.", "Typical session", "30 minutes", "Sessions played", "6", 30, 25);

    private static GameItem Peak() => new(SlotType.EndlessSquad,
        "PEAK", "/images/peak.png", "PEAK cover with climbers beneath a mountain",
        "Take on the next climb with your friends.", "Current objective", "Reach the summit", "Climbs played", "8", 60, 50);

    private static GameItem RvThereYet() => new(SlotType.EndlessSquad,
        "RV There Yet?", "/images/rv-there-yet.png", "RV There Yet? cover showing friends beside a camper van",
        "Get the camper back on the road with your group.", "Current objective", "Finish the road trip", "Sessions played", "3", 60, 50);

    private static GameItem Valorant() => new(SlotType.EndlessSquad,
        "Valorant", "/images/valorant.png", "Valorant cover featuring Jett on a red background",
        "Queue for a focused match with your team.", "Current objective", "Practice team play", "Matches played", "12", 45, 65);
}
