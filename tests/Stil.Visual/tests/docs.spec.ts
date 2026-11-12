// Accessibility checks with axe-core (WCAG 2.0/2.1, levels A and AA) on every page of the
// documentation site, in light and dark mode. The pages are read from the @page directives in
// docs/Stil.Docs/Pages, so a new page is checked without a change here.
import AxeBuilder from '@axe-core/playwright';
import { readdirSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { DOCS_URL, MODES, expect, settle, test } from './helpers';

const WCAG_TAGS = ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'];

function docsRoutes(): string[] {
    const dir = fileURLToPath(new URL('../../../docs/Stil.Docs/Pages', import.meta.url));
    return readdirSync(dir, { recursive: true, encoding: 'utf8' })
        .filter(file => file.endsWith('.razor'))
        .flatMap(file => [...readFileSync(join(dir, file), 'utf8').matchAll(/^@page "([^"{]+)"/gm)].map(m => m[1]))
        .sort();
}

for (const route of docsRoutes()) {
    for (const mode of MODES) {
        test(`axe: docs ${route}, ${mode}`, { tag: '@a11y' }, async ({ page }, testInfo) => {
            // The docs site follows the system color mode until a mode is picked.
            await page.emulateMedia({ colorScheme: mode });
            await page.goto(`${DOCS_URL}${route}`);
            await expect(page.locator('.docs-loading')).toHaveCount(0, { timeout: 45_000 });
            await expect(page.locator('main h1').first()).toBeVisible();
            await settle(page);

            const results = await new AxeBuilder({ page }).withTags(WCAG_TAGS).analyze();
            await testInfo.attach('axe-violations.json', {
                body: JSON.stringify(results.violations, null, 2),
                contentType: 'application/json',
            });
            const found = results.violations.flatMap(violation =>
                violation.nodes.map(node => `[${violation.impact}] ${violation.id}: ${node.target.join(' ')} (${violation.help})`));
            expect(found, `axe violations on the docs page ${route} (${mode})`).toEqual([]);
        });
    }
}
