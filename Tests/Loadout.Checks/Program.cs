using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using MidtermExam.Models;
using MidtermExam.Services;

var checks = 0;
void Check(bool condition, string scenario) { if (!condition) throw new Exception("FAIL: " + scenario); Console.WriteLine("PASS: " + scenario); checks++; }
var store = new AccountStore();
var account = store.Register(new() { DisplayName="Player One", Email="one@example.test", Password="test-password" })!;
Check(account.PasswordHash != "test-password", "accounts store a password hash");
Check(store.Verify("ONE@example.test", "test-password") == account, "email login is case-insensitive");
Check(store.Verify("one@example.test", "wrong") == null, "wrong password grants no account");
Check(store.Verify("unknown@example.test", "test-password") == null, "unknown login does not create an account");
Check(store.Register(new() { DisplayName="Duplicate", Email="ONE@example.test", Password="test-password" }) == null, "duplicate email is rejected");
var session = store.StartSession(account);
var auth = new AuthService(new FixedIdentity(session), store); await auth.InitializeAsync();
var loadout = new LoadoutService(auth);
Check(loadout.Slots.All(g => g == null), "new accounts start with empty slots");
var refreshed = new AuthService(new FixedIdentity(session), store); await refreshed.InitializeAsync();
Check(refreshed.CurrentUser.Id == auth.CurrentUser.Id, "a new circuit restores the same server session");
var game = SampleGames.Create()[0];
Check(loadout.EquipGame(0, game, out _), "an empty slot accepts a game");
Check(!loadout.EquipGame(0, SampleGames.Create()[0], out _), "occupied slots reject replacement");
Check(!loadout.EquipGame(3, SampleGames.Create()[0], out _), "a fourth slot cannot be added");
Check(new LoadoutService(refreshed).Slots[0]?.Id == game.Id, "refresh keeps game changes");
loadout.ChooseRecommendation(0); loadout.UpdateProgress(0,"55% complete","20 hours");
Check(loadout.Slots[0]?.PrimaryValue == "55% complete", "progress updates the active game");
_ = loadout.EvaluateTonight(15,0);
Check(loadout.ChosenSlotIndex == 0, "evaluating recommendations does not replace the chosen game");
var secondSession = store.StartSession(store.CreateDemo());
var secondAuth = new AuthService(new FixedIdentity(secondSession),store); await secondAuth.InitializeAsync();
var secondLoadout = new LoadoutService(secondAuth);
Check(secondLoadout.Slots[0]?.Id != game.Id && secondLoadout.Slots.Count(g => g != null) == 3, "sample sessions have their own seeded loadout");
Check(loadout.ClearSlot(0,"Finished",out var badge,out _), "completion frees an active slot");
Check(auth.CurrentUser.CurrentXp == 500 && badge != null && loadout.HallOfFame.Count == 1 && loadout.ChosenSlotIndex == null, "completion awards XP, badge, history and resets the chosen slot");
Check(!loadout.ClearSlot(0,null,out _,out _) && auth.CurrentUser.CurrentXp == 500, "repeated completion cannot award duplicate XP");
Check(secondLoadout.Slots[0] != null && secondAuth.CurrentUser.CurrentXp == 0, "one user's completion does not affect another user");
loadout.EquipGame(1,SampleGames.Create()[1],out _);
Check(loadout.DropSlot(1,"Moving on") && loadout.Graveyard.Count == 1 && auth.CurrentUser.CurrentXp == 500, "dropping records history and awards no XP");
var slots = SampleGames.Create();
foreach(var (minutes,energy,index) in new[] { (90,65,0),(89,65,2),(90,64,2),(45,100,1),(46,40,1),(46,41,2),(15,0,1),(180,100,0) })
    Check(RecommendationEvaluator.Evaluate(slots,minutes,energy).RecommendedIndex == index,$"recommendation boundary: {minutes} min, {energy} energy");
Check(RecommendationEvaluator.Evaluate(new GameItem?[3],45,35).Game == null,"empty loadouts return no recommendation");
Check(RecommendationEvaluator.Evaluate(new GameItem?[]{slots[0],null,null},15,0).Game?.Id == slots[0].Id,"partial loadouts fall back to an available game");
store.EndSession(session);
Check(!auth.IsAuthenticated,"logout invalidates the server session");
try { loadout.ChooseRecommendation(0); Check(false,"revoked session blocks mutations"); } catch(UnauthorizedAccessException) { Check(true,"revoked session blocks mutations"); }
Check(new AccountStore().FindSession(secondSession) == null,"server restart invalidates old sessions");
Console.WriteLine($"{checks} behavioral checks passed.");

sealed class FixedIdentity(string session) : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("session_id",session) },"test"))));
}
