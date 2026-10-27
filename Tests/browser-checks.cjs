const { chromium, expect } = require(process.env.PLAYWRIGHT_MODULE || 'playwright/test');
const fs = require('node:fs');
const path = require('node:path');
const base = process.env.LOADOUT_URL || 'http://localhost:5099';
const artifacts = process.env.LOADOUT_ARTIFACT_DIR || path.resolve('.artifacts');
fs.mkdirSync(artifacts, { recursive: true });
let checks = 0;
function check(condition, message) {
  if (!condition) throw new Error(message);
  checks++;
  console.log('PASS: ' + message);
}
async function ready(page) {
  await page.locator('[data-interactive="true"]').waitFor();
}
async function screenshot(page, name) {
  await page.evaluate(() => document.fonts.ready);
  if (await page.locator('[data-loadout-page][data-motion="active"]').count())
    await page.locator('[data-hero-motion="complete"]').waitFor();
  const modalOpen = await page.getByRole('dialog').isVisible().catch(() => false);
  if (!modalOpen) await page.evaluate(() => window.scrollTo(0, 0));
  await page.screenshot({ path: path.join(artifacts, name + '.png'), fullPage: !modalOpen });
}
async function range(page, name, value) {
  await page.getByRole('slider', { name, exact: true }).fill(String(value));
}
(async () => {
  const browser = await chromium.launch({
    executablePath: process.env.EDGE_PATH || 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',
    headless: true,
    chromiumSandbox: true
  });
  const errors = [];
  const context = await browser.newContext({ viewport: { width: 1366, height: 768 } });
  const page = await context.newPage();
  page.on('pageerror', error => errors.push(error.message));
  try {
    await page.goto(base + '/dashboard');
    await expect(page).toHaveURL(/\/login/);
    check(true, 'anonymous dashboard visit goes to login');
    await page.getByLabel('Email', { exact: true }).fill('unknown@example.test');
    await page.getByLabel('Password', { exact: true }).fill('test-password');
    await page.getByRole('button', { name: /^Log in/ }).click();
    await expect(page.getByRole('alert')).toContainText('Email or password is incorrect');
    check(await page.getByLabel('Email', { exact: true }).inputValue() === 'unknown@example.test', 'failed login preserves email');
    await page.goto(base + '/register');
    const email = `midterm-${Date.now()}@example.test`;
    await page.getByLabel('Display name', { exact: true }).fill('Mika');
    await page.getByLabel('Email', { exact: true }).fill(email);
    await page.getByLabel('Password', { exact: true }).fill('test-password');
    await page.getByLabel('Confirm password', { exact: true }).fill('different-password');
    check(await page.getByLabel('Confirm password', { exact: true }).evaluate(e => !e.checkValidity()), 'password mismatch is caught before submission');
    await page.getByLabel('Confirm password', { exact: true }).fill('test-password');
    await page.getByRole('button', { name: 'Show password', exact: true }).click();
    check(await page.getByLabel('Password', { exact: true }).getAttribute('type') === 'text', 'password visibility control works');
    await page.getByRole('button', { name: /^Create account/ }).click();
    await expect(page).toHaveURL(base + '/dashboard');
    await ready(page);
    await expect(page.getByRole('heading', { name: 'Add your first game.' })).toBeVisible();
    check(await page.getByRole('slider').count() === 0, 'new accounts start empty without inactive session controls');
    await screenshot(page, 'after-dashboard-empty');

    const addMain = page.getByRole('button', { name: 'Add game to Main campaign' });
    await addMain.click();
    const dialog = page.getByRole('dialog');
    await expect(dialog).toBeVisible();
    await expect(page.getByLabel('Game title', { exact: true })).toBeFocused();
    for (let i = 0; i < 12; i++) await page.keyboard.press('Tab');
    check(await dialog.evaluate(d => d.contains(document.activeElement)), 'keyboard focus stays inside the dialog');
    await page.keyboard.press('Escape');
    await expect(dialog).not.toBeVisible();
    await expect(addMain).toBeFocused();
    check(true, 'Escape closes dialog and restores focus');
    await addMain.click();
    await page.getByLabel('Game title', { exact: true }).fill('A Very Long Campaign Title That Still Needs to Fit Comfortably in the Active Game Slot');
    await page.getByText('Cover image', { exact: false }).click();
    await page.getByLabel('Image URL', { exact: true }).fill('not-a-url');
    await page.getByRole('button', { name: 'Add to loadout', exact: true }).click();
    await expect(dialog.getByRole('alert')).toContainText('http or https');
    check(true, 'invalid cover URL has a clear error');
    await page.getByLabel('Image URL', { exact: true }).fill(base + '/missing-cover.png');
    await page.getByRole('button', { name: 'Add to loadout', exact: true }).click();
    await expect(dialog).not.toBeVisible();
    await expect(page.getByRole('heading', { name: 'What fits tonight?' })).toBeVisible();
    await expect(page.locator('img[data-cover-fallback]').first()).toHaveAttribute('src', '/images/game-placeholder.svg');
    check(true, 'missing artwork uses the local fallback');
    await page.getByRole('button', { name: 'Choose for tonight', exact: true }).click();
    await expect(page.getByRole('button', { name: '✓ Chosen tonight', exact: true })).toBeDisabled();
    await range(page, 'Time available', 180); await range(page, 'Energy', 100);
    check(await page.getByText(/Chosen tonight:.*A Very Long/).count() === 1, 'session controls preserve an explicit game choice');
    await page.getByRole('button', { name: /^Update progress/ }).click();
    await page.getByLabel('Progress', { exact: true }).fill('61% complete');
    await page.getByLabel('Time played', { exact: true }).fill('24 hours');
    await page.getByRole('button', { name: 'Save progress', exact: true }).click();
    await expect(page.getByText('61% complete', { exact: true })).toBeVisible();
    await page.reload(); await ready(page);
    await expect(page.getByText('61% complete', { exact: true })).toBeVisible();
    await expect(page.getByRole('heading', { name: /Good to see you, Mika/ })).toBeVisible();
    check(true, 'refresh preserves sign-in, game progress, and chosen state');
    await page.getByText('•••', { exact: true }).click();
    await page.getByRole('button', { name: 'Mark completed', exact: true }).click();
    await page.getByRole('button', { name: 'Cancel', exact: true }).click();
    await expect(page.getByText('61% complete', { exact: true })).toBeVisible();
    check(true, 'canceling completion leaves the active game unchanged');
    await page.getByText('•••', { exact: true }).click();
    await page.getByRole('button', { name: 'Mark completed', exact: true }).click();
    await page.getByLabel('A note to remember it by', { exact: false }).fill('A good last chapter.');
    await screenshot(page, 'after-completion-dialog');
    await page.getByRole('button', { name: 'Mark completed', exact: true }).click();
    await expect(page.getByText(/Game completed. \+500 XP/)).toBeVisible();
    await page.getByRole('link', { name: 'History', exact: true }).click(); await ready(page);
    await expect(page.getByRole('heading', { name: 'Your game history', exact: true })).toBeVisible();
    await expect(page.getByText('A good last chapter.', { exact: true })).toBeVisible();
    await page.getByRole('button', { name: /^Dropped/ }).click();
    await expect(page.getByRole('heading', { name: 'No dropped games yet.' })).toBeVisible();
    await page.getByRole('button', { name: /^Completed/ }).click();
    await expect(page.getByText('A good last chapter.', { exact: true })).toBeVisible();
    check(true, 'completion history and its filters work');
    await page.getByRole('button', { name: /Mika Profile/ }).click();
    await expect(dialog.getByText('500 / 1000 XP', { exact: true })).toBeVisible();
    await expect(dialog.getByText('First Blood', { exact: true })).toBeVisible();
    await screenshot(page, 'after-profile');
    await page.keyboard.press('Escape');
    check(true, 'profile shows earned XP and badge');
    await page.getByRole('link', { name: 'Tonight', exact: true }).click(); await ready(page);
    await page.getByRole('button', { name: 'Add game to Quick play' }).click();
    await page.getByLabel('Game title', { exact: true }).fill('A short break');
    await page.getByText('Cover image', { exact: false }).click();
    await page.getByLabel('Or upload an image', { exact: true }).setInputFiles({ name: 'large.png', mimeType: 'image/png', buffer: Buffer.alloc(5 * 1024 * 1024 + 1) });
    await expect(dialog.getByRole('alert')).toContainText('too large');
    await page.getByLabel('Or upload an image', { exact: true }).setInputFiles({ name: 'invalid.png', mimeType: 'image/png', buffer: Buffer.from('This is not a real image file') });
    await expect(dialog.getByRole('alert')).toContainText("isn't a supported image");
    check(true, 'oversized and invalid image uploads are rejected');
    await page.getByLabel('Or upload an image', { exact: true }).setInputFiles({ name: 'small.png', mimeType: 'image/png', buffer: Buffer.from('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jRZkAAAAASUVORK5CYII=', 'base64') });
    await expect(dialog.getByText('Image ready.', { exact: true })).toBeVisible();
    await page.getByRole('button', { name: 'Add to loadout', exact: true }).click();
    await expect(dialog).not.toBeVisible();
    await page.getByText('•••', { exact: true }).click(); await page.getByRole('button', { name: 'Drop game', exact: true }).click();
    await page.getByLabel('Why are you moving on?', { exact: false }).fill('Ready for another game.');
    await page.getByRole('button', { name: 'Drop game', exact: true }).click();
    await page.getByRole('link', { name: 'History', exact: true }).click(); await ready(page);
    await expect(page.getByText('Ready for another game.', { exact: true })).toBeVisible();
    check(true, 'a valid image uploads and dropping records the reason');
    await screenshot(page, 'after-history');
    await page.getByRole('button', { name: 'Log out', exact: true }).click();
    await expect(page).toHaveURL(/\/login\?notice=loggedout/);
    await page.goto(base + '/dashboard'); await expect(page).toHaveURL(/\/login/);
    await page.getByLabel('Email', { exact: true }).fill(email); await page.getByLabel('Password', { exact: true }).fill('wrong-password');
    await page.getByRole('button', { name: /^Log in/ }).click(); await expect(page.getByRole('alert')).toContainText('incorrect');
    await page.getByLabel('Password', { exact: true }).fill('test-password'); await page.getByRole('button', { name: /^Log in/ }).click(); await ready(page);
    await page.getByRole('link', { name: 'History', exact: true }).click(); await ready(page);
    await expect(page.getByText('A good last chapter.', { exact: true })).toBeVisible();
    check(true, 'logout protects dashboard; returning login restores account history');
    const token = await page.locator('input[name="__RequestVerificationToken"]').first().inputValue();
    await context.request.post(base + '/auth/demo', { form: { __RequestVerificationToken: token } });
    await page.reload(); await ready(page);
    await expect(page.getByRole('heading', { name: 'Your game history' })).toBeVisible();
    await expect(page.getByText('A good last chapter.', { exact: true })).toBeVisible();
    check(await page.getByText('Sample demo', { exact: true }).count() === 0, 'sample entry does not replace a signed-in account');
    await page.getByRole('button', { name: 'Log out', exact: true }).click();
    await page.goto(base + '/register');
    await page.getByLabel('Display name', { exact: true }).fill('Duplicate'); await page.getByLabel('Email', { exact: true }).fill(email);
    await page.getByLabel('Password', { exact: true }).fill('test-password'); await page.getByLabel('Confirm password', { exact: true }).fill('test-password');
    await page.getByRole('button', { name: /^Create account/ }).click();
    await expect(page.getByRole('alert')).toContainText('already exists');
    check(true, 'duplicate registration has an actionable error');
    const blocked = await context.request.post(base + '/auth/demo', { form: {} });
    check(blocked.status() === 400, 'auth POST rejects a missing antiforgery token');

    const sampleContext = await browser.newContext({ viewport: { width: 1366, height: 768 } });
    const sample = await sampleContext.newPage(); sample.on('pageerror', error => errors.push(error.message));
    await sample.goto(base + '/login'); await sample.getByRole('button', { name: /^Try sample demo/ }).click(); await ready(sample);
    await expect(sample.getByText('Sample demo', { exact: true })).toBeVisible();
    await range(sample, 'Time available', 90); await range(sample, 'Energy', 65);
    await expect(sample.locator('.session-result').getByRole('heading', { name: 'Call of Duty', exact: true })).toBeVisible();
    check(await sample.getByText('A good last chapter.', { exact: true }).count() === 0, 'sample session does not inherit normal-account data');
    const otherContext = await browser.newContext(); const other = await otherContext.newPage();
    await other.goto(base + '/login'); await other.getByRole('button', { name: /^Try sample demo/ }).click(); await ready(other);
    await sample.getByText('•••', { exact: true }).first().click(); await sample.getByRole('button', { name: 'Drop game', exact: true }).click(); await sample.getByRole('button', { name: 'Drop game', exact: true }).click();
    await other.reload(); await ready(other);
    check(await other.getByRole('heading', { name: 'Call of Duty', exact: true }).count() === 1, 'two sample browsers have independent games');
    await otherContext.close();
    // Refill the slot so screenshots include all three roles.
    await sample.getByRole('button', { name: 'Add game to Main campaign' }).click();
    await sample.getByRole('button', { name: /Or try this sample/ }).click();
    await sample.getByRole('button', { name: 'Add to loadout', exact: true }).click();
    await expect(sample.getByRole('dialog')).not.toBeVisible();
    await sample.reload(); await ready(sample);
    await screenshot(sample, 'after-dashboard');
    for (const width of [390, 768, 1024, 1366]) {
      await page.setViewportSize({ width, height: 768 }); await sample.setViewportSize({ width, height: 768 });
      for (const [name, route] of [['landing', '/'], ['login', '/login'], ['register', '/register']]) {
        await page.goto(base + route); if (name === 'landing') await ready(page);
        await screenshot(page, `after-${name}-${width}`);
        check(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), `${name} has no horizontal overflow at ${width}px`);
      }
      await screenshot(sample, `after-dashboard-${width}`);
      check(await sample.evaluate(() => document.documentElement.scrollWidth <= innerWidth), `dashboard has no horizontal overflow at ${width}px`);
      await sample.getByRole('button', { name: /^Update progress/ }).first().click();
      await expect(sample.getByRole('dialog')).toBeVisible();
      check(await sample.getByRole('dialog').evaluate(d => d.getBoundingClientRect().width <= innerWidth && d.getBoundingClientRect().height <= innerHeight), `dialog fits at ${width}px`);
      await screenshot(sample, `after-dialog-${width}`); await sample.keyboard.press('Escape');
    }
    await sample.setViewportSize({ width: 683, height: 384 });
    check(await sample.evaluate(() => document.documentElement.scrollWidth <= innerWidth), 'dashboard reflows at the equivalent of 200% desktop zoom');
    await sample.emulateMedia({ reducedMotion: 'reduce' });
    check(await sample.evaluate(() => getComputedStyle(document.querySelector('button')).transitionDuration === '0s'), 'reduced motion removes interface transitions');
    check(errors.length === 0, 'no uncaught browser errors: ' + errors.join('; '));
    await sampleContext.close();
    console.log(`${checks} browser checks passed. Screenshots: ${artifacts}`);
  } catch (error) {
    await screenshot(page, 'failure'); throw error;
  } finally { await browser.close(); }
})().catch(error => { console.error(error.message); process.exitCode = 1; });
