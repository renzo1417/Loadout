# LOADOUT

A Blazor midterm project for keeping three games in rotation and choosing what to play based on your time and energy.

## Run

Requires the .NET 8 SDK (or a newer SDK that can target .NET 8).

```powershell
dotnet run
```

Open the localhost URL printed by the app. The launch profile uses `http://localhost:5099`.

## Walk through the app

1. Read the landing page or move the sliders in its sample recommendation.
2. Create an account. Your three slots start empty.
3. Add a game to Main campaign, Quick play, or Co-op / endless. Artwork and starting progress are optional. Try a role-specific sample using Call of Duty, Roblox, PEAK, RV There Yet?, or Valorant. Sample progress is illustrative.
4. Adjust time and energy, then choose a game for tonight. Adjusting the sliders never silently changes your explicit choice.
5. Update progress from the game slot. Open its more-actions menu to complete or drop it.
6. See the result and your notes in History. Completion awards 500 XP and can unlock badges in Profile.
7. Refresh or log out and log back in; your account data remains while the app is running.

For a quick presentation, use **Try sample demo** from the landing or login screen. Every sample session has independent games and a visible sample label. It does not replace an already signed-in account.

**Storage:** accounts, password hashes, games, and server sessions are held in memory. Restarting the app resets them and invalidates old sign-ins. There is no database, Steam connection, or automatic game launching.

## Verification

Package-free behavior checks:

```powershell
dotnet run --project Tests/Loadout.Checks/Loadout.Checks.csproj
```

Browser checks require a running app, Microsoft Edge, and the Playwright test tooling. If needed, install test tooling locally without adding a production dependency:

```powershell
npm install --no-save --package-lock=false @playwright/test
node Tests/browser-checks.cjs
```

Optional environment variables: `LOADOUT_URL`, `EDGE_PATH`, `PLAYWRIGHT_MODULE` (path to an existing `playwright/test` module), and `LOADOUT_ARTIFACT_DIR`. Browser captures default to the ignored `.artifacts` directory. The suite uses synthetic accounts and covers the full journey, validation, session isolation, image uploads, dialogs, responsive layouts, and reduced motion.

For a midterm usability check, ask someone unfamiliar with the project to explain its purpose from the landing screen and add their first game without coaching.

Landing motion and layout regression checks use the same browser tooling:

```powershell
node Tests/landing-checks.cjs
```

They check animation startup and cleanup, return navigation, reduced motion, missing-script fallback, shared alignment, and content-sized closing/footer sections. GSAP 3.12.5 and ScrollTrigger are served from `wwwroot/vendor/gsap` so the motion works without a CDN connection.
