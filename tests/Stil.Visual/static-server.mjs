// A tiny, dependency-free static file server for the visual tests. It serves the repository
// root, so the plain-HTML samples load ../../src/Stil.Css/dist/stil.css and ../../assets/
// exactly as they do from disk.
//
// Usage: node static-server.mjs [port]   (default 5181, or the PORT environment variable)

import { createReadStream } from 'node:fs';
import { stat } from 'node:fs/promises';
import { createServer } from 'node:http';
import { extname, join, normalize, resolve, sep } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = resolve(fileURLToPath(new URL('../..', import.meta.url)));
const port = Number(process.argv[2] ?? process.env.PORT ?? 5181);

const types = {
    '.html': 'text/html; charset=utf-8',
    '.css': 'text/css; charset=utf-8',
    '.js': 'text/javascript; charset=utf-8',
    '.mjs': 'text/javascript; charset=utf-8',
    '.json': 'application/json; charset=utf-8',
    '.svg': 'image/svg+xml',
    '.png': 'image/png',
    '.jpg': 'image/jpeg',
    '.jpeg': 'image/jpeg',
    '.gif': 'image/gif',
    '.webp': 'image/webp',
    '.ico': 'image/x-icon',
    '.woff': 'font/woff',
    '.woff2': 'font/woff2',
    '.txt': 'text/plain; charset=utf-8',
    '.map': 'application/json; charset=utf-8',
};

function send(response, status, text) {
    response.writeHead(status, { 'Content-Type': 'text/plain; charset=utf-8', 'Cache-Control': 'no-store' });
    response.end(text);
}

const server = createServer(async (request, response) => {
    if (request.method !== 'GET' && request.method !== 'HEAD') {
        send(response, 405, 'Method not allowed');
        return;
    }

    let pathname;
    try {
        pathname = decodeURIComponent(new URL(request.url ?? '/', 'http://localhost').pathname);
    } catch {
        send(response, 400, 'Bad request');
        return;
    }

    // Resolve inside the repository root only.
    let file = resolve(join(root, normalize(pathname)));
    if (file !== root && !file.startsWith(root + sep)) {
        send(response, 403, 'Forbidden');
        return;
    }

    try {
        let info = await stat(file);
        if (info.isDirectory()) {
            file = join(file, 'index.html');
            info = await stat(file);
        }
        response.writeHead(200, {
            'Content-Type': types[extname(file).toLowerCase()] ?? 'application/octet-stream',
            'Content-Length': info.size,
            'Cache-Control': 'no-store',
        });
        if (request.method === 'HEAD') {
            response.end();
            return;
        }
        createReadStream(file).pipe(response);
    } catch {
        send(response, 404, 'Not found');
    }
});

server.listen(port, '127.0.0.1', () => {
    console.log(`Static server for ${root} on http://127.0.0.1:${port}`);
});

for (const signal of ['SIGINT', 'SIGTERM']) {
    process.on(signal, () => server.close(() => process.exit(0)));
}
