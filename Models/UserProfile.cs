namespace MidtermExam.Models;

public sealed class UserProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = "Demo Operator";
    public string CallSign { get; set; } = "GHOST-01";
    public int Level { get; set; } = 1;
    public int CurrentXp { get; set; } = 250;
    public int XpToNextLevel => Level * 1000;
    public int XpPercentage => Math.Clamp((int)((double)CurrentXp / XpToNextLevel * 100), 0, 100);
    public int ClearedCount { get; set; } = 0;
    public int DroppedCount { get; set; } = 0;
    public DateTime MemberSince { get; set; } = DateTime.UtcNow;

    public List<Badge> Badges { get; set; } = new();

    public UserProfile() { }

    public UserProfile(string displayName, string email, string callSign)
    {
        Id = Guid.NewGuid();
        DisplayName = displayName;
        Email = email;
        Username = email;
        CallSign = callSign;
        Level = 1;
        CurrentXp = 0;
        ClearedCount = 0;
        DroppedCount = 0;
        MemberSince = DateTime.UtcNow;
    }

    public bool AddXp(int amount, out bool leveledUp, out Badge? newBadge)
    {
        CurrentXp += amount;
        leveledUp = false;
        newBadge = null;

        while (CurrentXp >= XpToNextLevel)
        {
            CurrentXp -= XpToNextLevel;
            Level++;
            leveledUp = true;
        }

        // Check for milestone badge unlocks
        if (ClearedCount == 1 && !Badges.Any(b => b.Id == "first_clear"))
        {
            newBadge = new Badge("first_clear", "First Blood", "Cleared your first backlog title.", "🎯", BadgeTier.Bronze);
            Badges.Add(newBadge);
        }
        else if (ClearedCount == 3 && !Badges.Any(b => b.Id == "hat_trick"))
        {
            newBadge = new Badge("hat_trick", "Hat Trick", "Cleared three distinct active slots.", "🏆", BadgeTier.Silver);
            Badges.Add(newBadge);
        }
        else if (Level >= 5 && !Badges.Any(b => b.Id == "veteran"))
        {
            newBadge = new Badge("veteran", "Loadout Veteran", "Achieved Level 5 operator rank.", "⭐", BadgeTier.Gold);
            Badges.Add(newBadge);
        }

        return leveledUp;
    }

    public void RecordClearedGame(GameItem game)
    {
        ClearedCount++;
    }

    public void RecordDroppedGame(GameItem game)
    {
        DroppedCount++;
    }
}
