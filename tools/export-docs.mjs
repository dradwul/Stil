// Exports the published docs site (docs/Stil.Docs.Host) as static files for GitHub Pages, which
// has no server: it runs the site once, saves its rendered pages as HTML, and writes the files
// that the server only answers as endpoints (fingerprinted aliases and the resource collection
// that WebAssembly starts from), so the static copy starts like the real one.
//
// Usage: node tools/export-docs.mjs <publish folder> <base path>
//   such as: node tools/export-docs.mjs site /Stil/
// The pages are written to <publish folder>/wwwroot, which is what Pages serves.
import { spawn } from 'node:child_process';
import { copyFileSync, existsSync, mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { dirname, join, resolve } from 'node:path';

const [site = 'site', base = '/'] = process.argv.slice(2);
const root = resolve(site);
const wwwroot = join(root, 'wwwroot');
const port = 5299;
const origin = `http://localhost:${port}`;

// The pages to save, by route; the home page is also 404.html, which Pages answers for any other
// address, so a deep link still starts the site and its router shows the page.
const routes = ['/'];

const server = spawn('dotnet', [join(root, 'Stil.Docs.Host.dll'), '--urls', origin, '--contentRoot', root], {
    stdio: ['ignore', 'ignore', 'inherit'],
    env: { ...process.env, ASPNETCORE_ENVIRONMENT: 'Production' },
});

try {
    await waitForServer();

    const pages = new Map();
    for (const route of routes) {
        const response = await fetch(origin + route);
        if (!response.ok) {
            throw new Error(`${route} answered ${response.status}.`);
        }

        const html = (await response.text()).replace(/<base href="\/"[^>]*>/, `<base href="${base}">`);
        if (!html.includes(`<base href="${base}">`) || !/<h1/.test(html)) {
            throw new Error(`${route} has no <base> to set, or no h1.`);
        }

        pages.set(route, html);
        write(join(wwwroot, route.replace(/^\//, ''), 'index.html'), html);
    }

    write(join(wwwroot, '404.html'), pages.get('/'));
    const aliases = writeAliases();
    const collections = await writeResourceCollections([...pages.values()]);
    console.log(`Exported ${pages.size} page(s), ${aliases} alias file(s) and ${collections} resource collection(s) to ${wwwroot}.`);
} finally {
    server.kill();
}

async function waitForServer() {
    for (let attempt = 0; attempt < 120; attempt++) {
        try {
            if ((await fetch(origin + '/')).ok) {
                return;
            }
        } catch {
            // Not listening yet.
        }

        await new Promise(done => setTimeout(done, 500));
    }

    throw new Error('The docs site did not start.');
}

// A static asset the server answers under another name (a fingerprinted address for a plain
// file, or the other way round) gets a copy under that name.
function writeAliases() {
    const manifest = JSON.parse(readFileSync(join(root, 'Stil.Docs.Host.staticwebassets.endpoints.json'), 'utf8'));
    let count = 0;
    for (const endpoint of manifest.Endpoints.filter(endpoint => endpoint.Selectors.length === 0)) {
        const target = join(wwwroot, endpoint.Route);
        const source = join(wwwroot, endpoint.AssetFile);
        if (!existsSync(target) && existsSync(source)) {
            mkdirSync(dirname(target), { recursive: true });
            copyFileSync(source, target);
            count++;
        }
    }

    return count;
}

// The resource collection's address is in the page's persisted component state, encoded twice.
async function writeResourceCollections(htmls) {
    const addresses = new Set();
    for (const html of htmls) {
        for (const [, state] of html.matchAll(/<!--Blazor-WebAssembly-Component-State:([A-Za-z0-9+/=]+)-->/g)) {
            for (const value of Object.values(JSON.parse(Buffer.from(state, 'base64').toString('utf8')))) {
                const text = Buffer.from(String(value), 'base64').toString('utf8');
                for (const [address] of text.matchAll(/_framework\/resource-collection[^"\\]*\.js/g)) {
                    addresses.add(address);
                }
            }
        }
    }

    for (const address of addresses) {
        const response = await fetch(`${origin}/${address}`);
        if (!response.ok) {
            throw new Error(`${address} answered ${response.status}.`);
        }

        write(join(wwwroot, address), await response.text());
    }

    return addresses.size;
}

function write(file, text) {
    mkdirSync(dirname(file), { recursive: true });
    writeFileSync(file, text);
}
