// Visual regression for the Blazor demo (samples/Stil.Demo): every gallery section, the
// orders page, and the open states of a dialog, a dropdown, an autocomplete and a sorted grid.
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { MODES, expect, expectAreaScreenshot, openDemo, test } from './helpers';

/** The ids of every `<section class="demo-section" aria-labelledby="…">` on the gallery page. */
function gallerySections(): string[] {
    const file = fileURLToPath(new URL('../../../samples/Stil.Demo/Components/Pages/Home.razor', import.meta.url));
    const razor = readFileSync(file, 'utf8');
    return [...razor.matchAll(/<section class="demo-section" aria-labelledby="([^"]+)"/g)].map(match => match[1]);
}

const sections = gallerySections();

test('the gallery has sections', { tag: '@visual' }, () => {
    expect(sections.length).toBeGreaterThan(8);
});

for (const mode of MODES) {
    test.describe(`demo gallery, ${mode}`, { tag: '@visual' }, () => {
        for (const id of sections) {
            test(`section ${id}`, async ({ page }) => {
                await openDemo(page, '/', mode);
                const section = page.locator(`section.demo-section[aria-labelledby="${id}"]`);
                await expect(section).toHaveScreenshot(`gallery-${id}-${mode}.png`);
            });
        }
    });

    test.describe(`demo open states, ${mode}`, { tag: '@visual' }, () => {
        test('dialog', async ({ page }) => {
            await openDemo(page, '/', mode);
            await page.getByRole('button', { name: 'Open dialog' }).click();
            const dialog = page.getByRole('dialog', { name: 'Invite a team member' });
            await expect(dialog).toBeVisible();
            await page.mouse.move(0, 0);
            // The viewport, so the backdrop is part of the baseline.
            await expect(page).toHaveScreenshot(`dialog-open-${mode}.png`);
        });

        test('dropdown', async ({ page }) => {
            await openDemo(page, '/', mode);
            const section = page.locator('section.demo-section[aria-labelledby="menus"]');
            const dropdown = section.locator('details.stil-dropdown').first();
            await dropdown.locator('summary').click();
            await expect(dropdown).toHaveAttribute('open', '');
            const menu = dropdown.locator('.stil-dropdown-menu');
            await expect(menu).toBeVisible();
            await page.mouse.move(0, 0);
            await expectAreaScreenshot(page, [dropdown.locator('summary'), menu], `dropdown-open-${mode}.png`);
        });

        test('autocomplete', async ({ page }) => {
            await openDemo(page, '/', mode);
            const input = page.getByRole('combobox', { name: 'City' });
            await input.click();
            const popup = page.locator('.stil-combobox', { has: input }).locator('.stil-combobox-popup');
            if (!(await popup.isVisible())) await input.press('ArrowDown');
            await expect(popup).toBeVisible();
            await expect(popup.getByRole('option').first()).toBeVisible();
            await page.mouse.move(0, 0);
            await expectAreaScreenshot(page, [input, popup], `autocomplete-open-${mode}.png`);
        });
    });

    test.describe(`demo orders, ${mode}`, { tag: '@visual' }, () => {
        test('page', async ({ page }) => {
            await openDemo(page, '/orders', mode);
            await expect(page.locator('.stil-data-grid tbody tr')).toHaveCount(10);
            await expect(page).toHaveScreenshot(`orders-${mode}.png`, { fullPage: true });
        });

        test('grid sorted by customer', async ({ page }) => {
            await openDemo(page, '/orders', mode);
            const grid = page.locator('.stil-data-grid');
            const header = grid.locator('th', { has: page.getByRole('button', { name: 'Customer' }) });
            await header.getByRole('button', { name: 'Customer' }).click();
            await expect(header).toHaveAttribute('aria-sort', 'ascending');
            await page.mouse.move(0, 0);
            await expect(grid).toHaveScreenshot(`orders-sorted-${mode}.png`);
        });
    });
}
