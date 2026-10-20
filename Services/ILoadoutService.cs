using MidtermExam.Models;

namespace MidtermExam.Services;

public interface ILoadoutService
{
    IReadOnlyList<GameItem?> Slots { get; }
    IReadOnlyList<GameItem> HallOfFame { get; }
    IReadOnlyList<GameItem> Graveyard { get; }
    int? ChosenSlotIndex { get; set; }
    int TimeAvailable { get; set; }
    int BrainBattery { get; set; }

    bool CanEquipSlot(int slotIndex);
    bool EquipGame(int slotIndex, GameItem game, out string? error);
    bool ClearSlot(int slotIndex, string? notes, out Badge? newBadge, out bool leveledUp);
    bool DropSlot(int slotIndex, string? reason);
    void UpdateProgress(int slotIndex, string primaryVal, string secondaryVal);
    RouletteResult EvaluateTonight(int timeAvailable, int brainBattery);
    void ChooseRecommendation(int slotIndex);
    void ResetToDefaults();

    event Action? OnLoadoutChanged;
}
