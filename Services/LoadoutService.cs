using MidtermExam.Models;

namespace MidtermExam.Services;

public sealed class LoadoutService : ILoadoutService
{
    private readonly IAuthService _authService;
    private readonly GameItem?[] _slots = new GameItem?[3];
    private readonly List<GameItem> _hallOfFame = new();
    private readonly List<GameItem> _graveyard = new();

    public IReadOnlyList<GameItem?> Slots => _slots;
    public IReadOnlyList<GameItem> HallOfFame => _hallOfFame;
    public IReadOnlyList<GameItem> Graveyard => _graveyard;

    public int? ChosenSlotIndex { get; set; }
    public int TimeAvailable { get; set; } = 45;
    public int BrainBattery { get; set; } = 35;

    public event Action? OnLoadoutChanged;

    public LoadoutService(IAuthService authService)
    {
        _authService = authService;
        ResetToDefaults();
    }

    public void ResetToDefaults()
    {
        _slots[0] = new GameItem(
            SlotType.PrimaryCampaign,
            "Obsidian Frontier",
            "/images/cover_campaign.jpg",
            "Obsidian Frontier campaign cover",
            "Your focused, story-driven commitment.",
            "Progress",
            "42% complete",
            "Time logged",
            "18.4 hours",
            targetSessionMinutes: 90,
            minBrainBattery: 65);

        _slots[1] = new GameItem(
            SlotType.PalateCleanser,
            "Osiris Protocol",
            "/images/cover_cleanser.jpg",
            "Osiris Protocol game cover",
            "A reliable short loop when focus is low.",
            "Average run",
            "18 minutes",
            "Runs finished",
            "34",
            targetSessionMinutes: 30,
            minBrainBattery: 25);

        _slots[2] = new GameItem(
            SlotType.EndlessSquad,
            "Strike Team: Obsidian",
            "/images/cover_coop.jpg",
            "Strike Team Obsidian game cover",
            "The standing choice for competitive or co-op play.",
            "Squad online",
            "2 players",
            "Weekly objective",
            "8 of 10",
            targetSessionMinutes: 60,
            minBrainBattery: 50);

        ChosenSlotIndex = null;
        OnLoadoutChanged?.Invoke();
    }

    public bool CanEquipSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Length) return false;
        return _slots[slotIndex] == null;
    }

    public bool EquipGame(int slotIndex, GameItem game, out string? error)
    {
        error = null;
        if (slotIndex < 0 || slotIndex >= _slots.Length)
        {
            error = "Invalid slot index.";
            return false;
        }

        // Enforce The Slot Lock
        if (_slots[slotIndex] != null)
        {
            error = $"The slot lock is active on this slot. Mark the current game ({_slots[slotIndex]!.Title}) as Cleared or Dropped first.";
            return false;
        }

        game.Slot = (SlotType)(slotIndex + 1);
        game.Status = GameStatus.Active;
        _slots[slotIndex] = game;

        OnLoadoutChanged?.Invoke();
        return true;
    }

    public bool ClearSlot(int slotIndex, string? notes, out Badge? newBadge, out bool leveledUp)
    {
        newBadge = null;
        leveledUp = false;

        if (slotIndex < 0 || slotIndex >= _slots.Length) return false;
        var game = _slots[slotIndex];
        if (game == null) return false;

        game.MarkCleared(notes);
        _hallOfFame.Insert(0, game);

        // Update User profile stats & award +500 XP
        var profile = _authService.CurrentUser;
        profile.RecordClearedGame(game);
        leveledUp = profile.AddXp(500, out var didLevelUp, out newBadge);

        // Free the slot
        _slots[slotIndex] = null;
        if (ChosenSlotIndex == slotIndex)
        {
            ChosenSlotIndex = null;
        }

        OnLoadoutChanged?.Invoke();
        return true;
    }

    public bool DropSlot(int slotIndex, string? reason)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Length) return false;
        var game = _slots[slotIndex];
        if (game == null) return false;

        game.MarkDropped(reason);
        _graveyard.Insert(0, game);

        var profile = _authService.CurrentUser;
        profile.RecordDroppedGame(game);

        // Free the slot
        _slots[slotIndex] = null;
        if (ChosenSlotIndex == slotIndex)
        {
            ChosenSlotIndex = null;
        }

        OnLoadoutChanged?.Invoke();
        return true;
    }

    public void UpdateProgress(int slotIndex, string primaryVal, string secondaryVal)
    {
        if (slotIndex >= 0 && slotIndex < _slots.Length && _slots[slotIndex] != null)
        {
            _slots[slotIndex]!.UpdateProgress(primaryVal, secondaryVal);
            OnLoadoutChanged?.Invoke();
        }
    }

    public RouletteResult EvaluateTonight(int timeAvailable, int brainBattery)
    {
        TimeAvailable = timeAvailable;
        BrainBattery = brainBattery;

        // Standard index recommendation rule
        int targetIndex = timeAvailable >= 90 && brainBattery >= 65
            ? 0
            : timeAvailable <= 45 || brainBattery <= 40
                ? 1
                : 2;

        // If target slot has an active game, use it.
        // Otherwise, find the best scoring active game in any slot.
        if (_slots[targetIndex] != null)
        {
            return BuildResult(targetIndex, _slots[targetIndex]!, timeAvailable, brainBattery);
        }

        // Fallback to highest suitability active game
        int bestIndex = -1;
        double bestScore = -1;

        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] != null)
            {
                double score = _slots[i]!.CalculateSuitabilityScore(timeAvailable, brainBattery);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }
        }

        if (bestIndex != -1)
        {
            return BuildResult(bestIndex, _slots[bestIndex]!, timeAvailable, brainBattery);
        }

        // If all slots are empty
        return new RouletteResult
        {
            RecommendedIndex = 0,
            Game = null,
            Role = "All slots open",
            Reason = "Equip a game into your loadout to generate tonight's recommendation.",
            MatchScore = 0
        };
    }

    private RouletteResult BuildResult(int index, GameItem game, int timeAvailable, int brainBattery)
    {
        string reason = index switch
        {
            0 => "You have enough time and focus to carry the campaign forward.",
            1 => "Its short loop fits the time and energy you have without adding another decision.",
            _ => "You have room for a flexible session without committing to a long campaign chapter."
        };

        return new RouletteResult
        {
            RecommendedIndex = index,
            Game = game,
            Role = game.Role,
            Reason = reason,
            MatchScore = game.CalculateSuitabilityScore(timeAvailable, brainBattery)
        };
    }

    public void ChooseRecommendation(int slotIndex)
    {
        ChosenSlotIndex = slotIndex;
        OnLoadoutChanged?.Invoke();
    }
}
