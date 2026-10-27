using MidtermExam.Models;
namespace MidtermExam.Services;
public sealed class LoadoutService(IAuthService auth) : ILoadoutService
{
    private LoadoutState State => auth.Account.Loadout;
    public IReadOnlyList<GameItem?> Slots { get { var s = State; lock(s.Sync) return s.Slots.ToArray(); } }
    public IReadOnlyList<GameItem> HallOfFame { get { var s = State; lock(s.Sync) return s.Completed.ToArray(); } }
    public IReadOnlyList<GameItem> Graveyard { get { var s = State; lock(s.Sync) return s.Dropped.ToArray(); } }
    public int? ChosenSlotIndex { get => State.ChosenSlotIndex; set => State.ChosenSlotIndex = value; }
    public int TimeAvailable { get => State.TimeAvailable; set => State.TimeAvailable = Math.Clamp(value, 15, 180); }
    public int BrainBattery { get => State.BrainBattery; set => State.BrainBattery = Math.Clamp(value, 0, 100); }
    public event Action? OnLoadoutChanged;
    public bool CanEquipSlot(int index) => index is >= 0 and < 3 && Slots[index] == null;
    public bool EquipGame(int index, GameItem game, out string? error)
    {
        error = null;
        var s = State;
        lock(s.Sync)
        {
            if (index is < 0 or > 2) { error = "Choose a valid slot."; return false; }
            if (s.Slots[index] != null) { error = "Complete or drop the current game before adding another."; return false; }
            if (string.IsNullOrWhiteSpace(game.Title) || game.Title.Length > 100 || game.TargetSessionMinutes is < 15 or > 240 || game.MinBrainBattery is < 0 or > 100)
            { error = "Check the game title, session length, and energy requirement."; return false; }
            game.Slot = (SlotType)(index + 1); game.Status = GameStatus.Active;
            s.Slots[index] = game;
        }
        OnLoadoutChanged?.Invoke(); return true;
    }
    public bool ClearSlot(int index, string? notes, out Badge? newBadge, out bool leveledUp)
    {
        newBadge = null; leveledUp = false;
        var account = auth.Account; var s = account.Loadout;
        lock(s.Sync)
        {
            if (index is < 0 or > 2 || s.Slots[index] is not { Status: GameStatus.Active } game) return false;
            game.MarkCleared(notes); s.Completed.Insert(0, game);
            account.Profile.RecordClearedGame(game); account.Profile.AddXp(500, out leveledUp, out newBadge);
            s.Slots[index] = null;
            if (s.ChosenSlotIndex == index) s.ChosenSlotIndex = null;
        }
        OnLoadoutChanged?.Invoke(); return true;
    }
    public bool DropSlot(int index, string? reason)
    {
        var account = auth.Account; var s = account.Loadout;
        lock(s.Sync)
        {
            if (index is < 0 or > 2 || s.Slots[index] is not { Status: GameStatus.Active } game) return false;
            game.MarkDropped(reason); s.Dropped.Insert(0, game); account.Profile.RecordDroppedGame(game);
            s.Slots[index] = null;
            if (s.ChosenSlotIndex == index) s.ChosenSlotIndex = null;
        }
        OnLoadoutChanged?.Invoke(); return true;
    }
    public void UpdateProgress(int index, string primary, string secondary)
    {
        var s = State;
        lock(s.Sync)
        {
            if (index is < 0 or > 2 || s.Slots[index] == null) return;
            s.Slots[index]!.UpdateProgress(primary.Trim(), secondary.Trim());
        }
        OnLoadoutChanged?.Invoke();
    }
    public RouletteResult EvaluateTonight(int minutes, int energy) => RecommendationEvaluator.Evaluate(Slots, minutes, energy);
    public void ChooseRecommendation(int index)
    {
        var s = State;
        lock(s.Sync) { if(index is < 0 or > 2 || s.Slots[index] == null) return; s.ChosenSlotIndex = index; }
        OnLoadoutChanged?.Invoke();
    }
    public void ResetToDefaults()
    {
        var account = auth.Account;
        if (!account.IsDemo) return;
        var s = account.Loadout;
        lock(s.Sync)
        {
            var samples = SampleGames.Create();
            for (var i = 0; i < 3; i++) s.Slots[i] = samples[i];
            s.Completed.Clear(); s.Dropped.Clear(); s.ChosenSlotIndex = null;
            s.TimeAvailable = 45; s.BrainBattery = 35;
            account.Profile.ClearedCount = 0; account.Profile.DroppedCount = 0;
            account.Profile.CurrentXp = 0; account.Profile.Level = 1; account.Profile.Badges.Clear();
        }
        OnLoadoutChanged?.Invoke();
    }
}
