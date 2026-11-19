// Exports the published docs site (docs/Stil.Docs.Host) as static files for GitHub Pages, which
// has no server. It runs the site once and saves:
//   - every page in its sitemap, rendered, as <path>.html (Pages serves /components/buttons from
//     components/buttons.html, with status 200), and the home page as index.html;
//   - 404.html, the not-found page, for any other address;
//   - sitemap.xml and robots.txt, which the site builds with StilSitemap and StilRobots;
//   - the files the server only answers as endpoints (fingerprinted aliases, and the resource
//     collection WebAssembly starts from), so the static copy starts like the real one.
// The pages are rendered at http://localhost, so their addresses (canonical, og:url) are rewritten
// to the site's public address, and <base href> to its path.
//
// Usage: node tools/export-docs.mjs <publish folder> <site address>
//   such as: node tools/export-docs.mjs site https://dradwul.github.io/Stil/
// The files are written to <publish folder>/wwwroot, which is what Pages serves.
import { spawn } from 'node:child_process';
import { copyFileSync, existsSync, mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { dirname, join, resolve } from 'node:path';

const [site = 'site', address = 'http://localhost/'] = process.argv.slice(2);
const siteUrl = address.endsWith('/') ? address : `${address}/`;
const base = new URL(siteUrl).pathname;
const root = resolve(site);
const wwwroot = join(root, 'wwwroot');
const port = 5299;
const origin = `http://localhost:${port}`;

const server = spawn('dotnet', [join(root, 'Stil.Docs.Host.dll'), '--urls', origin, '--contentRoot', root], {
    stdio: ['ignore', 'ignore', 'inherit'],
    env: { ...process.env, ASPNETCORE_ENVIRONMENT: 'Production', Docs__SiteUrl: siteUrl },
});

try {
    await waitForServer();

    const sitemap = await text('/sitemap.xml');
    write(join(wwwroot, 'sitemap.xml'), sitemap);
    write(join(wwwroot, 'robots.txt'), await text('/robots.txt'));

    const routes = [...sitemap.matchAll(/<loc>([^<]+)<\/loc>/g)].map(([, loc]) => {
        if (!loc.startsWith(siteUrl)) {
            throw new Error(`The sitemap has ${loc}, outside ${siteUrl}.`);
        }

        return loc.slice(siteUrl.length);
    });

    const pages = [];
    for (const route of routes) {
        const html = publish(await text(`/${route}`), route);
        pages.push(html);
        write(join(wwwroot, route === '' ? 'index.html' : `${route}.html`), html);
    }

    const notFound = await fetch(`${origin}/this-address-has-no-page`);
    if (notFound.status !== 404) {
        throw new Error(`An unknown address answered ${notFound.status}, not 404.`);
    }

    write(join(wwwroot, '404.html'), publish(await notFound.text(), null));
    const aliases = writeAliases();
    const collections = await writeResourceCollections(pages);
    console.log(`Exported ${pages.length} pages, 404.html, sitemap.xml, robots.txt, ${aliases} alias files and ${collections} resource collection(s) for ${siteUrl}.`);
} finally {
    server.kill();
}

// A rendered page as Pages serves it: its base path and its public addresses.
function publish(html, route) {
    const published = html
        .replace(/<base href="\/"[^>]*>/, `<base href="${base}">`)
        .replaceAll(`${origin}/`, siteUrl);
    if (!published.includes(`<base href="${base}">`) || !/<h1/.test(published)) {
        throw new Error(`${route ?? '404.html'} has no <base> to set, or no h1.`);
    }

    if (route !== null && !published.includes(`<link rel="canonical" href="${siteUrl}${route}"`)) {
        throw new Error(`${route} has no canonical address ${siteUrl}${route}.`);
    }

    return published;
}

async function text(path) {
    const response = await fetch(origin + path);
    if (!response.ok) {
        throw new Error(`${path} answered ${response.status}.`);
    }

    return response.text();
}

async function waitForServer() {
    for (let attempt = 0; attempt < 120; attempt++) {
        try {
            if ((await fetch(`${origin}/robots.txt`)).ok) {
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
                const decoded = Buffer.from(String(value), 'base64').toString('utf8');
                for (const [collection] of decoded.matchAll(/_framework\/resource-collection[^"\\]*\.js/g)) {
                    addresses.add(collection);
                }
            }
        }
    }

    for (const collection of addresses) {
        write(join(wwwroot, collection), await text(`/${collection}`));
    }

    return addresses.size;
}

function write(file, content) {
    mkdirSync(dirname(file), { recursive: true });
    writeFileSync(file, content);
}
