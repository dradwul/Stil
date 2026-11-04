// Checks that Stil's default tokens meet WCAG AA contrast in light and dark mode, and that the
// derived colors (auto text color, ink and accent) hold up for a range of brand colors.
// Usage: node tools/check-contrast.mjs [path/to/source/dir]
import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';

const sourceDir = process.argv[2] ?? fileURLToPath(new URL('../src/Stil.Css/src', import.meta.url));
const css = ['tokens.css', 'derived.css'].map(file => readFileSync(join(sourceDir, file), 'utf8')).join('\n');

// The derived-color formulas below mirror src/derived.css. If a constant changes there, change it here.
const FG_THRESHOLD = 0.1791; // luminance where black and white text give equal contrast
const FG_DARK = 0.0466; // linear luminance of the dark text, #3d3d3d
const FG_RATIO = 4.6; // the contrast the dark text keeps, darkening toward black when it must
const ACCENT_MAX_LIGHT = 0.15; // accent luminance cap in light mode
const ACCENT_MIN_DARK = 1 - 0.7; // accent luminance floor in dark mode
for (const constant of ['(0.1791 -', '/ 4.6 - 0.05, 0.0466)', '0.15 / (', '0.7 / (']) {
    if (!css.includes(constant)) {
        fail(`derived.css no longer contains "${constant}". Update tools/check-contrast.mjs to match.`);
    }
}

// Sample brand colors a theme might use, in addition to Stil's own defaults.
const SAMPLE_COLORS = [
    '#7048e8', '#d0bfff', '#2b8a3e', '#b2f2bb', '#e8590c', '#ffd8a8',
    '#000000', '#ffffff', '#808080', '#ff0000', '#00ff00', '#0000ff', '#ffff00',
].map(parseHex);

const tokens = parseRootTokens(css);
const modes = { light: 0, dark: 1 };
const variants = ['primary', 'secondary', 'success', 'danger', 'warning', 'info'];
const failures = [];
let checks = 0;

function check(label, foreground, background, minimum) {
    checks++;
    const ratio = contrast(foreground, background);
    if (ratio < minimum) {
        failures.push(`${label}: ${ratio.toFixed(2)}:1 (needs ${minimum}:1)`);
    }
}

for (const [mode, index] of Object.entries(modes)) {
    const color = name => {
        const value = tokens[name];
        if (!value) fail(`Token --stil-color-${name} is missing.`);
        return value[Math.min(index, value.length - 1)];
    };
    const backgrounds = ['body-bg', 'surface', 'surface-muted'];

    for (const bg of backgrounds) {
        check(`${mode}: text on ${bg}`, color('text'), color(bg), 4.5);
        check(`${mode}: text-muted on ${bg}`, color('text-muted'), color(bg), 4.5);
        check(`${mode}: border-strong on ${bg}`, color('border-strong'), color(bg), 3);

        // Ink (outline buttons, error text, toast edges) and accent use the same formula.
        for (const brand of [...variants.map(color), ...SAMPLE_COLORS]) {
            check(`${mode}: ink from ${toHex(brand)} on ${bg}`, accent(brand, mode), color(bg), 4.5);
        }
    }

    // Subtle alerts and badges: normal text on a 15% tint of the variant over the surface.
    for (const variant of variants) {
        const tint = mix(color(variant), color('surface'), 0.15);
        check(`${mode}: text on subtle ${variant}`, color('text'), tint, 4.5);
    }
}

for (const background of [...variants.map(v => tokens[v][0]), ...SAMPLE_COLORS]) {
    check(`auto text on ${toHex(background)}`, autoForeground(background), background, 4.5);
}

if (failures.length > 0) {
    fail(`${failures.length} of ${checks} contrast checks failed:\n  ${failures.join('\n  ')}`);
}
console.log(`All ${checks} contrast checks passed.`);

// --- Formulas (mirroring stil.css) -------------------------------------------------------

function autoForeground(background) {
    const l = luminance(background);
    if (l < FG_THRESHOLD) {
        return [1, 1, 1];
    }

    const gray = fromLinear(Math.min(FG_DARK, Math.max(0, (l + 0.05) / FG_RATIO - 0.05)));
    return [gray, gray, gray];
}

function accent(primary, mode) {
    const linear = primary.map(toLinear);
    const y = luminanceOfLinear(linear);
    if (mode === 'light') {
        const factor = Math.min(1, ACCENT_MAX_LIGHT / (y + 0.0001));
        return linear.map(c => c * factor).map(fromLinear);
    }
    const factor = Math.min(1, (1 - ACCENT_MIN_DARK) / (1.0001 - y));
    return linear.map(c => 1 - (1 - c) * factor).map(fromLinear);
}

// color-mix(in srgb, a p, b): linear interpolation of the gamma-encoded channels.
function mix(a, b, p) {
    return a.map((c, i) => c * p + b[i] * (1 - p));
}

// --- Color math --------------------------------------------------------------------------

function parseRootTokens(source) {
    const root = source.match(/:root\s*\{([\s\S]*?)\n\s*\}/);
    if (!root) fail('Could not find the :root token block in stil.css.');
    const result = {};
    for (const [, name, value] of root[1].matchAll(/--stil-color-([\w-]+):\s*([^;]+);/g)) {
        const hexes = [...value.matchAll(/#[0-9a-f]{6}\b/gi)].map(m => parseHex(m[0]));
        if (hexes.length > 0) result[name] = hexes;
    }
    return result;
}

function parseHex(hex) {
    return [1, 3, 5].map(i => parseInt(hex.slice(i, i + 2), 16) / 255);
}

function toHex(rgb) {
    return '#' + rgb.map(c => Math.round(c * 255).toString(16).padStart(2, '0')).join('');
}

function toLinear(c) {
    return c <= 0.04045 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4;
}

function fromLinear(c) {
    return c <= 0.0031308 ? c * 12.92 : 1.055 * c ** (1 / 2.4) - 0.055;
}

function luminanceOfLinear([r, g, b]) {
    return 0.2126 * r + 0.7152 * g + 0.0722 * b;
}

function luminance(rgb) {
    return luminanceOfLinear(rgb.map(toLinear));
}

function contrast(a, b) {
    const [high, low] = [luminance(a), luminance(b)].sort((x, y) => y - x);
    return (high + 0.05) / (low + 0.05);
}

function fail(message) {
    console.error(message);
    process.exit(1);
}
