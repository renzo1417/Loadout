const { chromium, expect } = require(process.env.PLAYWRIGHT_MODULE || 'playwright/test');
const fs = require('node:fs');
const path = require('node:path');
const base = process.env.LOADOUT_URL || 'http://localhost:5099';
const artifacts = process.env.LOADOUT_ARTIFACT_DIR || path.resolve('.artifacts');
fs.mkdirSync(artifacts, { recursive: true });
let checks = 0;
const check = (condition, label) => { if (!condition) throw new Error(label); checks++; console.log('PASS: ' + label); };
async function frames(page) { await page.evaluate(() => new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)))); }
async function ready(page) { await page.locator('[data-interactive="true"]').waitFor(); await page.evaluate(() => document.fonts.ready); }
async function settled(page) {
  await frames(page);
  await page.waitForFunction(() => !window.gsap || !gsap.getTweensOf(document.querySelectorAll('[data-reveal-title], [data-reveal-copy]')).some(tween => tween.isActive()));
}
(async () => {
  const browser = await chromium.launch({ executablePath: process.env.EDGE_PATH || 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', headless: true, chromiumSandbox: true });
  const errors = [];
  try {
    const context = await browser.newContext({ viewport: { width: 1366, height: 768 } });
    const page = await context.newPage(); page.on('pageerror', error => errors.push(error.message));
    await page.goto(base); await ready(page);
    await expect(page.locator('[data-loadout-page]')).toHaveAttribute('data-motion', 'active');
    await expect(page.locator('[data-loadout-page]')).toHaveAttribute('data-hero-motion', 'complete');
    check(await page.evaluate(() => gsap.version === '3.12.5' && ScrollTrigger.getAll().length > 0), 'the original GSAP runtime initializes real landing animations');
    check(await page.locator('[data-hero-word]').evaluate(element => getComputedStyle(element).transform === 'none'), 'wordmark entrance completes in its natural position');
    check(await page.evaluate(() => performance.getEntriesByType('resource').filter(entry => /gsap|ScrollTrigger/.test(entry.name)).every(entry => new URL(entry.name).origin === location.origin)), 'animation runtime is served locally');
    await page.screenshot({ path: path.join(artifacts, 'landing-motion-desktop.png') });

    const visual = page.locator('[data-story-step]').first().locator('[data-reveal-media]');
    const position = await visual.evaluate(element => element.getBoundingClientRect().top + scrollY);
    await page.evaluate(top => { scrollTo(0, top - 650); ScrollTrigger.update(); }, position); await frames(page);
    const before = await visual.evaluate(element => Number(gsap.getProperty(element, 'yPercent')));
    await page.evaluate(top => { scrollTo(0, top - 120); ScrollTrigger.update(); }, position); await frames(page);
    const after = await visual.evaluate(element => Number(gsap.getProperty(element, 'yPercent')));
    check(before !== after, 'story artwork responds to scrolling');

    await page.locator('.mechanics__columns').scrollIntoViewIfNeeded(); await settled(page);
    const columns = await page.locator('.mechanics__columns article').evaluateAll(items => items.map(item => ({ title:item.querySelector('h3').getBoundingClientRect().top, copy:item.querySelector('p').getBoundingClientRect().top, ending:item.querySelector('strong').getBoundingClientRect().bottom })));
    for (const key of ['title', 'copy', 'ending']) check(Math.max(...columns.map(item => item[key])) - Math.min(...columns.map(item => item[key])) < 2, `mechanics ${key} baselines align`);

    await page.locator('.final-cta').scrollIntoViewIfNeeded(); await settled(page);
    const closing = await page.evaluate(() => {
      const section = document.querySelector('.final-cta').getBoundingClientRect();
      const heading = document.querySelector('.final-cta h2').getBoundingClientRect();
      const button = document.querySelector('.final-cta .button').getBoundingClientRect();
      const footer = document.querySelector('.loadout-footer').getBoundingClientRect();
      return { height:section.height, footer:footer.height, baseline:Math.abs(heading.bottom-button.bottom), gap:footer.top-section.bottom };
    });
    check(closing.height < 500 && closing.footer < 240, 'closing section and footer have content-sized heights');
    check(closing.baseline < 12, 'closing heading and action align along the bottom');
    check(Math.abs(closing.gap) < 2, 'closing section meets the footer without a blank spacer');
    console.log('Closing dimensions: ' + JSON.stringify(closing));
    await page.locator('.final-cta').screenshot({ path: path.join(artifacts, 'after-closing-desktop.png') });

    for (const width of [320, 390, 768, 1024, 1366, 1920, 2560]) {
      await page.setViewportSize({ width, height:768 }); await page.goto(base); await ready(page);
      await expect(page.locator('[data-loadout-page]')).toHaveAttribute('data-hero-motion', 'complete');
      const geometry = await page.evaluate(() => {
        const word=document.querySelector('[data-hero-word]');
        const clip=word.parentElement.getBoundingClientRect();
        const style=getComputedStyle(word);
        const marker=document.createElement('span');
        marker.style.cssText='display:inline-block;width:0;height:0;padding:0;margin:0;border:0;vertical-align:baseline';
        word.append(marker);
        const baseline=marker.getBoundingClientRect().bottom;
        marker.remove();
        const context=document.createElement('canvas').getContext('2d');
        context.font=style.fontWeight+' '+style.fontSize+' '+style.fontFamily;
        const ink=context.measureText(word.textContent.trim());
        const range=document.createRange();range.selectNodeContents(word);
        const text=range.getBoundingClientRect();
        const glyphsFit=text.left>=clip.left-1&&text.right<=clip.right+1&&baseline-ink.actualBoundingBoxAscent>=clip.top-1&&baseline+ink.actualBoundingBoxDescent<=clip.bottom+1;
        const copy=document.querySelector('.hero__message').getBoundingClientRect();
        const frame=document.querySelector('.page-frame').getBoundingClientRect();
        const preview=document.querySelector('.hero__interface').getBoundingClientRect();
        return { glyphsFit, word:word.scrollWidth<=word.clientWidth, page:document.documentElement.scrollWidth<=innerWidth, edge:Math.abs(copy.left-frame.left), preview:preview.left>=0&&preview.right<=innerWidth };
      });
      check(geometry.word && geometry.page && geometry.preview, `hero and page fit at ${width}px`);
      check(geometry.glyphsFit, `all LOADOUT letter bounds remain inside the reveal container at ${width}px`);
      check(geometry.edge < 2, `hero and section edges align at ${width}px`);
      await page.screenshot({ path:path.join(artifacts, `landing-aligned-${width}.png`) });
      await page.locator('.final-cta').scrollIntoViewIfNeeded(); await settled(page);
      check(await page.locator('.final-cta .button').isVisible(), `closing action remains visible at ${width}px`);
      await page.locator('.final-cta').screenshot({ path:path.join(artifacts, `closing-aligned-${width}.png`) });
    }

    await page.goto(base); await ready(page); await page.getByRole('link', { name:'Log in', exact:true }).first().click();
    await expect(page).toHaveURL(/\/login/);
    await expect.poll(() => page.evaluate(() => ScrollTrigger.getAll().length)).toBe(0);
    check(await page.evaluate(() => loadoutEngine.root === null), 'navigation cleans up all landing animations and observers');
    await page.getByRole('link', { name:'Back to home' }).click(); await ready(page);
    await expect(page.locator('[data-loadout-page]')).toHaveAttribute('data-motion','active');
    check(await page.evaluate(() => ScrollTrigger.getAll().length > 0), 'landing animations initialize again on return');

    await page.emulateMedia({ reducedMotion:'reduce' });
    await expect(page.locator('[data-loadout-page]')).toHaveAttribute('data-motion','reduced');
    check(await page.evaluate(() => ScrollTrigger.getAll().length === 0 && [...document.querySelectorAll('[data-reveal-title], [data-hero-word]')].every(element => Number(getComputedStyle(element).opacity) === 1)), 'reduced motion removes movement and keeps content visible');
    await context.close();

    const fallback = await browser.newContext();
    const staticPage = await fallback.newPage();
    staticPage.on('pageerror', error => errors.push(error.message));
    await staticPage.route('**/vendor/gsap/**', route => route.abort());
    await staticPage.goto(base); await ready(staticPage);
    await expect(staticPage.locator('[data-loadout-page]')).toHaveAttribute('data-motion','unavailable');
    check(await staticPage.getByRole('heading', { level:1 }).isVisible(), 'landing remains readable if animation assets fail');
    check(await staticPage.evaluate(() => [...document.querySelectorAll('[data-reveal-title]')].every(element => Number(getComputedStyle(element).opacity) === 1)), 'script failure never leaves headings hidden');
    await fallback.close();
    check(errors.length === 0, 'no uncaught browser errors: ' + errors.join('; '));
    console.log(`${checks} landing checks passed.`);
  } finally { await browser.close(); }
})().catch(error => { console.error(error.message); process.exitCode=1; });
