// Visual regression for the plain-HTML samples (samples/Stil.Html): one screenshot per
// component section of the showcase, and the whole admin page.
import { MODES, expect, openShowcase, showcaseSections, test, type Mode, type Palette } from './helpers';

const sections = showcaseSections();

test('the showcase has component sections', { tag: '@visual' }, () => {
    expect(sections.length).toBeGreaterThan(20);
});

for (const mode of MODES) {
    test.describe(`showcase, ${mode}`, { tag: '@visual' }, () => {
        for (const id of sections) {
            test(`section ${id}`, async ({ page }) => {
                await openShowcase(page, 'index.html', mode);
                await expect(page.locator(`section.component-group#${id}`)).toHaveScreenshot(`${id}-${mode}.png`);
            });
        }
    });
}

// Sections with brand-colored parts, in non-default palettes.
const paletteChecks: { palette: Palette; mode: Mode }[] = [
    { palette: 'purple', mode: 'light' },
    { palette: 'purple', mode: 'dark' },
    { palette: 'sunset', mode: 'light' },
];
const paletteSections = ['buttons', 'forms', 'alerts', 'badges', 'tabs'];

for (const { palette, mode } of paletteChecks) {
    test.describe(`showcase, ${palette} palette, ${mode}`, { tag: '@visual' }, () => {
        for (const id of paletteSections) {
            test(`section ${id}`, async ({ page }) => {
                await openShowcase(page, 'index.html', mode, palette);
                await expect(page.locator(`section.component-group#${id}`)).toHaveScreenshot(`${id}-${palette}-${mode}.png`);
            });
        }
    });
}

for (const mode of MODES) {
    test(`admin page, ${mode}`, { tag: '@visual' }, async ({ page }) => {
        await openShowcase(page, 'admin.html', mode);
        await expect(page).toHaveScreenshot(`admin-${mode}.png`, { fullPage: true });
    });
}
