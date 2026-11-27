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
            // The page is rendered on the server, so its content is there before WebAssembly starts.
            await page.goto(`${DOCS_URL}${route}`);
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

// The Razor or HTML switch of the examples (ExampleHtml, DocsCodeChoice): the HTML is rendered from
// the example, the choice applies to every example and is remembered, and the switched page passes axe.
test('axe: docs example code as HTML', { tag: '@a11y' }, async ({ page }) => {
    await page.goto(`${DOCS_URL}/components/buttons`);
    await expect(page.locator('main h1').first()).toBeVisible();
    const examples = page.locator('.docs-example');
    await examples.locator('details').evaluateAll(details => details.forEach(d => { (d as HTMLDetailsElement).open = true; }));

    // WebAssembly starts on the first pointer move or key, so the switch answers once it runs.
    const first = examples.first();
    const html = first.getByRole('button', { name: 'HTML', exact: true });
    await page.mouse.move(10, 10);
    await expect(async () => {
        await html.click();
        await expect(html).toHaveAttribute('aria-pressed', 'true', { timeout: 1_000 });
    }).toPass({ timeout: 120_000 });

    await expect(first.locator('.docs-code-language')).toHaveText('html');
    await expect(first.locator('pre code')).toContainText('<button type="button" class="stil-btn stil-btn-primary">Primary</button>');
    await expect(examples.nth(1).locator('.docs-code-language')).toHaveText('html');

    const results = await new AxeBuilder({ page }).withTags(WCAG_TAGS).include('main').analyze();
    expect(results.violations.map(violation => violation.id), 'axe violations with the code as HTML').toEqual([]);

    // The choice is remembered on the next page.
    await page.goto(`${DOCS_URL}/components/cards`);
    await page.locator('details.docs-example-source').first().evaluate(d => { (d as HTMLDetailsElement).open = true; });
    await page.mouse.move(20, 20);
    await expect(page.locator('.docs-example').first().locator('.docs-code-language')).toHaveText('html', { timeout: 120_000 });
});
