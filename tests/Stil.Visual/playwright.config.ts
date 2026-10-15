import { defineConfig } from '@playwright/test';
import { DEMO_URL, STATIC_ORIGIN, STATIC_PORT } from './tests/helpers';

const isCI = !!process.env.CI;

// Locally the installed Microsoft Edge is used, so no browser download is needed. CI uses
// Playwright's bundled Chromium (installed in the official Playwright Docker image).
const browser = isCI ? { browserName: 'chromium' as const } : { browserName: 'chromium' as const, channel: 'msedge' };

export default defineConfig({
    testDir: './tests',
    outputDir: './test-results',
    // Baselines are per platform: fonts and anti-aliasing differ between Windows and Linux,
    // so both sets can live side by side.
    snapshotPathTemplate: '{testDir}/__screenshots__/{platform}/{projectName}/{testFilePath}/{arg}{ext}',

    fullyParallel: true,
    forbidOnly: isCI,
    retries: isCI ? 1 : 0,
    workers: isCI ? 2 : 4,
    timeout: 60_000,
    reporter: isCI
        ? [['list'], ['html', { open: 'never', outputFolder: 'playwright-report' }], ['github']]
        : [['list'], ['html', { open: 'never', outputFolder: 'playwright-report' }]],

    expect: {
        timeout: 10_000,
        toHaveScreenshot: {
            // Small, but tolerant of anti-aliasing differences between runs.
            maxDiffPixelRatio: 0.002,
            threshold: 0.2,
            animations: 'disabled',
            caret: 'hide',
            scale: 'css',
            // Stops infinite animations (spinners) and hides text carets during screenshots.
            stylePath: './screenshot.css',
        },
    },

    use: {
        ...browser,
        locale: 'en-US',
        timezoneId: 'Europe/Stockholm',
        colorScheme: 'light',
        reducedMotion: 'reduce',
        deviceScaleFactor: 1,
        actionTimeout: 15_000,
        navigationTimeout: 30_000,
        trace: 'retain-on-failure',
        launchOptions: {
            // More stable glyph rendering between runs.
            args: ['--font-render-hinting=none', '--disable-lcd-text', '--force-color-profile=srgb'],
        },
    },

    projects: [
        {
            name: 'desktop',
            use: { viewport: { width: 1280, height: 800 } },
        },
        {
            name: 'mobile',
            use: { viewport: { width: 390, height: 844 }, isMobile: true, hasTouch: true },
            // Accessibility checks do not depend on the viewport; they run once, on desktop.
            grepInvert: /@a11y/,
        },
    ],

    webServer: [
        {
            command: `node static-server.mjs ${STATIC_PORT}`,
            url: `${STATIC_ORIGIN}/samples/Stil.Html/index.html`,
            reuseExistingServer: !isCI,
            timeout: 30_000,
            stdout: 'ignore',
            stderr: 'pipe',
        },
        {
            command: 'dotnet run --project ../../samples/Stil.Demo --launch-profile http',
            url: DEMO_URL,
            reuseExistingServer: !isCI,
            // The first run restores and builds the solution.
            timeout: 300_000,
            stdout: 'ignore',
            stderr: 'pipe',
            env: {
                // Same number and date formatting on every machine, whatever the OS culture.
                DOTNET_SYSTEM_GLOBALIZATION_INVARIANT: '1',
                DOTNET_CLI_TELEMETRY_OPTOUT: '1',
                DOTNET_NOLOGO: '1',
            },
        },
    ],
});
