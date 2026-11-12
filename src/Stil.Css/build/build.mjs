// Builds dist/stil.css and dist/stil.min.css from the files in src/ and the generated parts,
// dist/stil-blocks.css and stil-blocks.min.css (the opt-in page blocks) from src/blocks/, and
// copies the optional behaviours module src/stil.js to dist/stil.js.
//   node build/build.mjs           build once
//   node build/build.mjs --watch   rebuild when a source file changes
//   node build/build.mjs --check   fail if dist/ is out of date (used by CI)
import { readFileSync, writeFileSync, mkdirSync, watch } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import browserslist from 'browserslist';
import { browserslistToTargets, transform } from 'lightningcss';
import { generateGrid, generateIcons, generateNavbarExpand, generateUtilities, generateVariants } from './generate.mjs';

const root = join(dirname(fileURLToPath(import.meta.url)), '..');
const src = join(root, 'src');
const dist = join(root, 'dist');
const pkg = JSON.parse(readFileSync(join(root, 'package.json'), 'utf8'));

// Layer order and the files in each layer. Order within a layer matters: variants come last
// in stil.components so they override each component's defaults.
const layers = [
    ['stil.tokens', ['tokens.css', 'derived.css']],
    ['stil.reset', ['reset.css']],
    ['stil.base', ['base.css']],
    ['stil.components', [
        'layout.css',
        generateGrid,
        'components/button.css',
        'components/form.css',
        'components/content.css',
        'components/navigation.css',
        generateNavbarExpand,
        'components/overlay.css',
        'components/data.css',
        'components/pickers.css',
        'components/inputs.css',
        'components/media.css',
        'components/tree.css',
        'components/calendar.css',
        'components/extras.css',
        generateIcons,
        generateVariants,
    ]],
    ['stil.utilities', ['utilities.css', generateUtilities]],
];

// The page blocks (website sections): a separate, opt-in stylesheet loaded after stil.css. Its
// rules join the stil.components layer, so the utilities still win over them.
const blocks = ['blocks/sections.css', 'blocks/selling.css'];

const banner = `/*! Stil CSS v${pkg.version} | MIT License | https://github.com/dradwul/Stil */\n`;
const blocksBanner = `/*! Stil CSS blocks v${pkg.version} | MIT License | https://github.com/dradwul/Stil */\n`;
const targets = browserslistToTargets(browserslist(pkg.browserslist));

// Git may check the sources out with CRLF line endings on Windows, and the pretty output keeps a
// line break inside a value as it is, so make them LF first.
const read = source => readFileSync(join(src, source), 'utf8').replace(/\r\n/g, '\n');

function bundle() {
    const parts = [`@layer ${layers.map(([name]) => name).join(', ')};`];
    for (const [layer, sources] of layers) {
        const body = sources
            .map(source => (typeof source === 'function' ? source() : read(source)))
            .join('\n\n');
        parts.push(`@layer ${layer} {\n${body}\n}`);
    }
    return parts.join('\n\n');
}

// The pretty and the minified output of one stylesheet.
function css(filename, source) {
    const options = { filename, code: Buffer.from(source), targets, errorRecovery: false };
    const pretty = transform({ ...options, minify: false });
    const minified = transform({ ...options, minify: true });
    for (const warning of [...pretty.warnings, ...minified.warnings]) {
        console.warn(`warning: ${warning.message}`);
    }
    return [pretty.code.toString(), minified.code.toString()];
}

function compile() {
    const [stil, stilMin] = css('stil.css', bundle());
    const [blocksCss, blocksMin] = css('stil-blocks.css', `@layer stil.components {\n${blocks.map(read).join('\n\n')}\n}`);
    return {
        'stil.css': banner + stil,
        'stil.min.css': banner + stilMin,
        'stil-blocks.css': blocksBanner + blocksCss,
        'stil-blocks.min.css': blocksBanner + blocksMin,
        'stil.js': banner + read('stil.js'),
    };
}

function build() {
    const output = compile();
    mkdirSync(dist, { recursive: true });
    for (const [name, content] of Object.entries(output)) {
        writeFileSync(join(dist, name), content);
        console.log(`dist/${name}  ${(Buffer.byteLength(content) / 1024).toFixed(1)} kB`);
    }
}

function check() {
    const output = compile();
    const stale = Object.entries(output).filter(([name, content]) => {
        // Git may check dist/ out with CRLF line endings on Windows; compare content only.
        try { return readFileSync(join(dist, name), 'utf8').replace(/\r\n/g, '\n') !== content; } catch { return true; }
    });
    if (stale.length > 0) {
        console.error(`dist/ is out of date (${stale.map(([name]) => name).join(', ')}). Run "npm run build" in src/Stil.Css and commit the result.`);
        process.exit(1);
    }
    console.log('dist/ is up to date.');
}

const args = process.argv.slice(2);
if (args.includes('--check')) {
    check();
} else {
    build();
    if (args.includes('--watch')) {
        let timer;
        console.log('Watching src/ and build/ for changes…');
        const rebuild = () => {
            clearTimeout(timer);
            timer = setTimeout(() => {
                try { build(); } catch (error) { console.error(error.message); }
            }, 100);
        };
        watch(src, { recursive: true }, rebuild);
        watch(join(root, 'build'), rebuild);
    }
}
