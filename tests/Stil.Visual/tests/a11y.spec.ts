// Accessibility checks with axe-core (WCAG 2.0/2.1, levels A and AA) on the plain-HTML samples
// and the Blazor demo, in light and dark mode. Any violation fails the test, except the known
// issues listed below.
import AxeBuilder from '@axe-core/playwright';
import type { Page } from '@playwright/test';
import { MODES, expect, openDemo, openShowcase, test, type Mode } from './helpers';

const WCAG_TAGS = ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'];

type Violations = Awaited<ReturnType<AxeBuilder['analyze']>>['violations'];

interface Target {
    name: string;
    open: (page: Page, mode: Mode) => Promise<void>;
}

const targets: Target[] = [
    { name: 'showcase index.html', open: (page, mode) => openShowcase(page, 'index.html', mode) },
    { name: 'showcase admin.html', open: (page, mode) => openShowcase(page, 'admin.html', mode) },
    { name: 'showcase calendar.html', open: (page, mode) => openShowcase(page, 'calendar.html', mode) },
    { name: 'showcase site.html', open: (page, mode) => openShowcase(page, 'site.html', mode) },
    { name: 'showcase blog.html', open: (page, mode) => openShowcase(page, 'blog.html', mode) },
    { name: 'showcase article.html', open: (page, mode) => openShowcase(page, 'article.html', mode) },
    { name: 'demo gallery', open: (page, mode) => openDemo(page, '/', mode) },
    { name: 'demo orders', open: (page, mode) => openDemo(page, '/orders', mode) },
    {
        name: 'demo orders, new order row being edited',
        open: async (page, mode) => {
            await openDemo(page, '/orders', mode);
            await page.getByRole('button', { name: 'New order' }).click();
            await expect(page.locator('tr.stil-data-grid-editing')).toBeVisible();
        },
    },
    {
        name: 'demo orders, edit dialog open',
        open: async (page, mode) => {
            await openDemo(page, '/orders', mode);
            // The row's actions menu is a <details> dropdown.
            const menu = page.locator('details', { has: page.locator('[aria-label="Actions for order 1064"]') });
            await menu.locator('summary').click();
            await menu.getByText('Edit all fields…').click();
            await expect(page.getByRole('dialog', { name: 'Edit order #1064' })).toBeVisible();
        },
    },
];

interface KnownIssue {
    target: string;
    rule: string;
    /** Matches the axe selector of each excluded node. */
    selector: RegExp;
    reason: string;
}

/**
 * Known violations, excluded node by node so every other rule and element is still checked.
 * Each one also has a tracking test below, marked test.fail(): once the issue is fixed, that
 * test passes unexpectedly and fails the run, as a reminder to delete the entry here.
 */
const knownIssues: KnownIssue[] = [];

function isKnown(target: string, rule: string, selector: string): boolean {
    return knownIssues.some(issue => issue.target === target && issue.rule === rule && issue.selector.test(selector));
}

/** One line per failing node, so the report names the rule, impact and selector. */
function describe(violations: Violations): string[] {
    return violations.flatMap(violation =>
        violation.nodes.map(node => ({ violation, selector: node.target.join(' ') })))
        .map(({ violation, selector }) => `[${violation.impact}] ${violation.id}: ${selector} (${violation.help})`);
}

function unexpected(target: string, violations: Violations): string[] {
    return violations.flatMap(violation =>
        violation.nodes
            .map(node => node.target.join(' '))
            .filter(selector => !isKnown(target, violation.id, selector))
            .map(selector => `[${violation.impact}] ${violation.id}: ${selector} (${violation.help})`));
}

for (const target of targets) {
    for (const mode of MODES) {
        test(`axe: ${target.name}, ${mode}`, { tag: '@a11y' }, async ({ page }, testInfo) => {
            await target.open(page, mode);
            const results = await new AxeBuilder({ page }).withTags(WCAG_TAGS).analyze();

            await testInfo.attach('axe-violations.json', {
                body: JSON.stringify(results.violations, null, 2),
                contentType: 'application/json',
            });
            const known = describe(results.violations).length - unexpected(target.name, results.violations).length;
            if (known > 0) {
                testInfo.annotations.push({ type: 'known axe issues', description: `${known} node(s) excluded, see knownIssues` });
            }
            expect(unexpected(target.name, results.violations), `axe violations on ${target.name} (${mode})`).toEqual([]);
        });
    }
}

// Tracking tests for the known issues: expected to fail until the issue is fixed.
for (const issue of knownIssues) {
    const target = targets.find(t => t.name === issue.target)!;
    test(`axe known issue: ${issue.target}, ${issue.rule}`, { tag: '@a11y' }, async ({ page }) => {
        test.fail(true, `Known issue: ${issue.reason}. Remove it from knownIssues when this test starts passing.`);
        await target.open(page, 'light');
        const results = await new AxeBuilder({ page }).withRules([issue.rule]).analyze();
        const matching = results.violations
            .flatMap(violation => violation.nodes.map(node => node.target.join(' ')))
            .filter(selector => issue.selector.test(selector));
        expect(matching).toEqual([]);
    });
}
