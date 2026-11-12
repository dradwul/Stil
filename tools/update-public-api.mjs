// Updates the public API baseline of the packages (src/*/PublicAPI.*.txt) after a change to the
// public API, from what the build reports:
//   - a public API that is not listed (RS0016) is added to PublicAPI.Unshipped.txt;
//   - a listed API that is gone (RS0017) is removed from PublicAPI.Unshipped.txt, or, if a
//     released version has it (PublicAPI.Shipped.txt), marked *REMOVED* there in Unshipped.
// Usage: node tools/update-public-api.mjs
//
// Read the diff afterwards: every line in it is a change to what the packages offer. Visual
// Studio and Rider do the same with the "Add to public API" fix on the RS0016 error.
import { execFileSync } from 'node:child_process';
import { readFileSync, writeFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';

const root = fileURLToPath(new URL('..', import.meta.url));
// In dependency order: the others reference Stil.Blazor, which must build first.
const projects = ['Stil.Blazor', 'Stil.Charts', 'Stil.Markdown', 'Stil.RichText'];

const read = file => readFileSync(file, 'utf8').split(/\r?\n/).filter(line => line && line !== '#nullable enable');
const write = (file, lines) => {
    const sorted = [...new Set(lines)].sort((a, b) => (a < b ? -1 : a > b ? 1 : 0));
    writeFileSync(file, ['#nullable enable', ...sorted, ''].join('\n'));
};

for (const project of projects) {
    const dir = `${root}src/${project}/`;
    let output;
    try {
        output = execFileSync('dotnet', ['build', dir, '--no-incremental', '-clp:NoSummary'], { encoding: 'utf8', maxBuffer: 1 << 28 });
    } catch (error) {
        output = error.stdout;
    }

    const ours = [...output.matchAll(/^.*$/gm)].map(m => m[0]).filter(line => line.includes(`${project}.csproj]`));
    const added = new Set(ours.flatMap(line => [...line.matchAll(/error RS0016: Symbol '(.+?)' is not part of the declared public API/g)].map(m => m[1])));
    const removed = new Set(ours.flatMap(line => [...line.matchAll(/error RS0017: Symbol '(.+?)' is part of the declared API/g)].map(m => m[1])));

    // With other errors, the analyzer may not see the whole API, so change nothing.
    const otherErrors = ours.filter(line => / error /.test(line) && !/ error RS001[67]:/.test(line));
    if (otherErrors.length > 0) {
        console.error([...new Set(otherErrors)].slice(0, 10).join('\n'));
        console.error(`${project} has other build errors. Fix them and run this again.`);
        process.exit(1);
    }

    const shipped = new Set(read(`${dir}PublicAPI.Shipped.txt`));
    const unshipped = read(`${dir}PublicAPI.Unshipped.txt`).filter(line => !removed.has(line));
    for (const symbol of removed) {
        if (shipped.has(symbol)) unshipped.push(`*REMOVED*${symbol}`);
    }
    write(`${dir}PublicAPI.Unshipped.txt`, [...unshipped, ...added]);
    console.log(`${project}: ${added.size} added, ${removed.size} removed`);
}
