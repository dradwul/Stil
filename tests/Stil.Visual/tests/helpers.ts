import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { test as base, expect, type Locator, type Page } from '@playwright/test';

export { expect };

/** Port of static-server.mjs, which serves the repository root. */
export const STATIC_PORT = Number(process.env.STIL_STATIC_PORT ?? 5181);
export const STATIC_ORIGIN = `http://127.0.0.1:${STATIC_PORT}`;
/** The Blazor demo's http launch profile (samples/Stil.Demo/Properties/launchSettings.json). */
export const DEMO_URL = 'http://localhost:5100';

export type Mode = 'light' | 'dark';
export const MODES: Mode[] = ['light', 'dark'];

/** Palettes of the showcase theme picker (samples/Stil.Html/theme-picker.js). */
export type Palette = 'teal' | 'purple' | 'forest' | 'sunset';

/**
 * A test whose browser context only talks to the two local servers. Everything else (the
 * Google Fonts stylesheet the samples load) is aborted, so the pages render with the
 * fallback font on every run instead of depending on the network.
 */
export const test = base.extend({
    context: async ({ context }, use) => {
        await context.route(
            url => url.hostname !== '127.0.0.1' && url.hostname !== 'localhost',
            route => route.abort('blockedbyclient'),
        );
        await use(context);
    },
});

/** The ids of every `<section class="component-group">` in the plain-HTML showcase. */
export function showcaseSections(): string[] {
    const file = fileURLToPath(new URL('../../../samples/Stil.Html/index.html', import.meta.url));
    const html = readFileSync(file, 'utf8');
    return [...html.matchAll(/<section class="component-group" id="([^"]+)"/g)].map(match => match[1]);
}

/** Waits until web fonts and images have loaded. */
export async function settle(page: Page): Promise<void> {
    await page.evaluate(async () => {
        await document.fonts.ready;
        await Promise.all(
            [...document.images].filter(image => !image.complete).map(
                image => new Promise(resolve => { image.onload = image.onerror = resolve; }),
            ),
        );
    });
}

/**
 * Opens a page of samples/Stil.Html with the given color mode and palette. The theme picker
 * reads its choice from localStorage, so it is set before the page loads.
 */
export async function openShowcase(page: Page, file: string, mode: Mode, palette: Palette = 'teal'): Promise<void> {
    await page.addInitScript(([m, p]) => {
        try {
            localStorage.setItem('stil-showcase-mode', m);
            localStorage.setItem('stil-showcase-palette', p);
        } catch { /* storage blocked: the picker falls back to light/teal */ }
    }, [mode, palette]);
    await page.goto(`${STATIC_ORIGIN}/samples/Stil.Html/${file}`);
    await expect(page.locator('html')).toHaveAttribute('data-stil-theme', mode);
    await settle(page);
}

/**
 * Opens a page of the Blazor demo and selects the color mode with the navbar's "Color mode"
 * select. Changing the select only works once the interactive (Blazor Server) circuit is
 * connected, so this also proves the page is interactive before a test clicks anything.
 * Light mode is selected by switching to dark and back.
 *
 * The value is set with a change event instead of selectOption(): below the md breakpoint the
 * select sits in the collapsed navbar, and opening the menu before the circuit is connected
 * races with the interactive render (which closes it again).
 */
export async function openDemo(page: Page, path: string, mode: Mode): Promise<void> {
    await page.goto(`${DEMO_URL}${path}`);
    const shell = page.locator('.demo-shell');
    const select = page.getByLabel('Color mode');
    const steps: Mode[] = mode === 'light' ? ['dark', 'light'] : ['dark'];

    await expect(async () => {
        for (const step of steps) {
            await select.evaluate((element, value) => {
                const control = element as HTMLSelectElement;
                control.value = value;
                control.dispatchEvent(new Event('change', { bubbles: true }));
            }, step === 'light' ? 'Light' : 'Dark');
            await expect(shell).toHaveAttribute('data-stil-theme', step, { timeout: 2_000 });
        }
    }).toPass({ timeout: 45_000, intervals: [250, 500, 1_000] });

    await page.mouse.move(0, 0);
    await settle(page);
}

/**
 * A screenshot of the area covering all the given elements plus some padding, for open
 * popups (menus, listboxes) that overflow their component's own box.
 */
export async function expectAreaScreenshot(page: Page, locators: Locator[], name: string, padding = 16): Promise<void> {
    // Centre the first element so a popup below or above it fits in the viewport.
    await locators[0].evaluate(element => element.scrollIntoView({ block: 'center', inline: 'nearest' }));
    const boxes = [];
    for (const locator of locators) {
        const box = await locator.boundingBox();
        if (!box) throw new Error(`No bounding box for ${locator}`);
        boxes.push(box);
    }
    const viewport = page.viewportSize()!;
    const x = Math.max(0, Math.floor(Math.min(...boxes.map(b => b.x)) - padding));
    const y = Math.max(0, Math.floor(Math.min(...boxes.map(b => b.y)) - padding));
    const right = Math.min(viewport.width, Math.ceil(Math.max(...boxes.map(b => b.x + b.width)) + padding));
    const bottom = Math.min(viewport.height, Math.ceil(Math.max(...boxes.map(b => b.y + b.height)) + padding));
    await expect(page).toHaveScreenshot(name, { clip: { x, y, width: right - x, height: bottom - y } });
}
