using MidtermExam.Models;
namespace MidtermExam.Services;
public static class RecommendationEvaluator
{
    public static RouletteResult Evaluate(IReadOnlyList<GameItem?> slots, int minutes, int energy)
    {
        var target = minutes >= 90 && energy >= 65 ? 0 : minutes <= 45 || energy <= 40 ? 1 : 2;
        var index = target < slots.Count && slots[target] is { Status: GameStatus.Active } ? target : -1;
        if (index < 0)
            index = Enumerable.Range(0, slots.Count).Where(i => slots[i] is { Status: GameStatus.Active })
                .OrderByDescending(i => slots[i]!.CalculateSuitabilityScore(minutes, energy)).DefaultIfEmpty(-1).First();
        if (index < 0) return new() { Reason = "Add a game to get your first recommendation." };
        var game = slots[index]!;
        var reason = index == target ? index switch
        {
            0 => "You have the time and focus to make progress in your main campaign.",
            1 => "A shorter session fits the time or energy you have right now.",
            _ => "A flexible session fits without committing to a long story chapter."
        } : "Of the games in your loadout, this is the closest fit for your time and energy.";
        return new() { RecommendedIndex = index, Game = game, Role = game.Role, Reason = reason,
            MatchScore = game.CalculateSuitabilityScore(minutes, energy) };
    }
}
