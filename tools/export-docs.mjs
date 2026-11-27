// Exports the published docs site (docs/Stil.Docs.Host) as static files for GitHub Pages, which
// has no server. It runs the site once and saves:
//   - every page in its sitemap, rendered, as <path>.html (Pages serves /components/buttons from
//     components/buttons.html, with status 200), and the home page as index.html;
//   - 404.html, the not-found page, for any other address;
//   - sitemap.xml and robots.txt, which the site builds with StilSitemap and StilRobots;
//   - the files the server only answers as endpoints (fingerprinted aliases, and the resource
//     collection WebAssembly starts from), so the static copy starts like the real one;
//   - for AI assistants (https://llmstxt.org/): every page as Markdown beside its HTML
//     (components/buttons.md, and index.html.md for the home page), llms.txt, which links to them
//     in the order of the sidebar with their descriptions, and llms-full.txt, every page in one file.
// The pages are rendered at http://localhost, so their addresses (canonical, og:url) are rewritten
// to the site's public address, and <base href> to its path.
//
// Usage: node tools/export-docs.mjs <publish folder> <site address>
//   such as: node tools/export-docs.mjs site https://dradwul.github.io/Stil/
// The files are written to <publish folder>/wwwroot, which is what Pages serves.
import { spawn } from 'node:child_process';
import { copyFileSync, existsSync, mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { dirname, join, resolve } from 'node:path';
import { parse, pageToMarkdown } from './html-to-markdown.mjs';

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

    const llms = writeLlmsFiles(routes, pages);

    const notFound = await fetch(`${origin}/this-address-has-no-page`);
    if (notFound.status !== 404) {
        throw new Error(`An unknown address answered ${notFound.status}, not 404.`);
    }

    write(join(wwwroot, '404.html'), publish(await notFound.text(), null));
    const aliases = writeAliases();
    const collections = await writeResourceCollections(pages);
    console.log(`Exported ${pages.length} pages, 404.html, sitemap.xml, robots.txt, ${aliases} alias files and ${collections} resource collection(s) for ${siteUrl}.`);
    console.log(`For AI assistants: ${pages.length} Markdown pages, llms.txt (${kilobytes(llms.index)}) and llms-full.txt (${kilobytes(llms.full)}).`);
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

// The Markdown copies of the pages, and llms.txt and llms-full.txt, which list them in the order of
// the sidebar (the pages outside it come last).
function writeLlmsFiles(routes, htmls) {
    const pages = routes.map((route, index) => {
        const html = htmls[index];
        const markdownPath = route === '' ? 'index.html.md' : `${route}.md`;
        const markdown = pageToMarkdown(html, siteUrl + route);
        write(join(wwwroot, markdownPath), markdown);
        return {
            route,
            markdown,
            url: siteUrl + markdownPath,
            title: /^# (.*)$/m.exec(markdown)?.[1] ?? route,
            description: decodeAttribute(/<meta name="description" content="([^"]*)"/.exec(html)?.[1] ?? ''),
        };
    });

    const byRoute = new Map(pages.map(page => [page.route, page]));
    for (const page of pages) {
        for (const [, target] of page.markdown.matchAll(/\]\(([^)#\s]+)/g)) {
            if (target.startsWith(siteUrl) && !byRoute.has(target.slice(siteUrl.length))) {
                throw new Error(`${page.route || 'The home page'} links to ${target}, which was not exported.`);
            }
        }
    }

    const sections = navSections(htmls[routes.indexOf('')]).map(section => ({
        title: section.title,
        // The sidebar's name for a page, such as "Introduction" for the home page.
        pages: section.links.map(({ route, title }) => {
            if (!byRoute.has(route)) {
                throw new Error(`The sidebar links to ${route}, which was not exported.`);
            }

            return { ...byRoute.get(route), title };
        }),
    }));
    const listed = new Set(sections.flatMap(section => section.pages.map(page => page.route)));
    const others = pages.filter(page => !listed.has(page.route));
    if (others.length) {
        sections.push({ title: 'Other pages', pages: others });
    }

    const intro = [
        '# Stil',
        '',
        '> Stil is a themeable CSS library (`stil-css` on npm) and a component library for Blazor (`Stil` on NuGet)',
        '> built on it, with charts (`Stil.Charts`), Markdown (`Stil.Markdown`), rich text (`Stil.RichText`) and',
        '> project templates (`Stil.Templates`). It is free and open source (MIT), for .NET 10.',
        '',
        'Use Stil components (`StilButton`, `StilDataGrid<TItem>`, `StilTextBox` and so on) and `stil-*` CSS classes,',
        'not Bootstrap classes or inline styles. Colors are `StilVariant` values (Primary, Secondary, Success, Danger,',
        'Warning, Info), and a theme is a set of `--stil-*` CSS tokens. Start a new app with',
        '`dotnet new install Stil.Templates` and `dotnet new stil-app`, or add `Stil` to an existing app as the page',
        'Install Stil for Blazor shows. Without Blazor, use the CSS on its own (`npm install stil-css`).',
    ].join('\n');

    const index = [
        intro,
        ...sections.map(section => [
            `## ${section.title}`,
            '',
            ...section.pages.map(page => `- [${page.title}](${page.url})${page.description ? `: ${page.description}` : ''}`),
        ].join('\n')),
        ['## Optional', '', `- [All the docs in one file](${siteUrl}llms-full.txt): every page above, in this order.`].join('\n'),
    ].join('\n\n') + '\n';

    const full = [
        intro,
        ...sections.flatMap(section => section.pages).map(page =>
            page.markdown.replace(/^(# .*)\n/, `$1\n\nSource: ${siteUrl}${page.route}\n`).trimEnd()),
    ].join('\n\n---\n\n') + '\n';

    write(join(wwwroot, 'llms.txt'), index);
    write(join(wwwroot, 'llms-full.txt'), full);
    return { index, full };
}

// The sidebar's groups and the routes they link to, read from a rendered page.
function navSections(html) {
    const all = (node, test, found = []) => {
        if (test(node)) {
            found.push(node);
        }

        for (const child of node.children ?? []) {
            all(child, test, found);
        }

        return found;
    };
    const classes = node => (node.attrs?.class ?? '').split(/\s+/);
    const textOf = node => node.text ?? (node.children ?? []).map(textOf).join('');
    const nav = all(parse(html), node => node.tag === 'nav' && classes(node).includes('docs-nav'))[0];
    if (!nav) {
        throw new Error('The home page has no sidebar (nav.docs-nav).');
    }

    return all(nav, node => classes(node).includes('stil-nav-group')).map(group => ({
        title: textOf(all(group, node => classes(node).includes('stil-nav-group-title'))[0]).trim(),
        links: all(group, node => node.tag === 'a' && node.attrs.href !== undefined).map(link => ({
            route: new URL(link.attrs.href, siteUrl).href.slice(siteUrl.length),
            title: textOf(link).trim(),
        })),
    }));
}

function decodeAttribute(value) {
    return parse(value).children.map(node => node.text ?? '').join('');
}

function kilobytes(content) {
    return `${Math.round(Buffer.byteLength(content) / 1024)} kB`;
}

function write(file, content) {
    mkdirSync(dirname(file), { recursive: true });
    writeFileSync(file, content);
}
