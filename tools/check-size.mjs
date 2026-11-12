// Checks the shipped stylesheets and script against a size budget, so they cannot grow by
// accident. The budget is on the gzipped size, which is what a browser downloads.
// Usage: node tools/check-size.mjs
//
// When a file grows on purpose (a new component, say), raise its budget in the list below in
// the same pull request, so the growth is a decision that shows in the review.
import { readFileSync } from 'node:fs';
import { gzipSync } from 'node:zlib';
import { fileURLToPath } from 'node:url';

const budgets = [
    // [file, budget in KiB, gzipped]
    ['src/Stil.Css/dist/stil.min.css', 25],
    ['src/Stil.Css/dist/stil.js', 24],
    ['src/Stil.Charts/wwwroot/stil-charts.css', 4.5],
];

const root = fileURLToPath(new URL('..', import.meta.url));
const kib = bytes => (bytes / 1024).toFixed(1) + ' KiB';
let failed = false;

for (const [file, budget] of budgets) {
    const content = readFileSync(root + file);
    const gzipped = gzipSync(content, { level: 9 }).length;
    const share = Math.round(gzipped / (budget * 1024) * 100);
    const over = gzipped > budget * 1024;
    failed ||= over;
    console.log(`${over ? 'OVER' : 'ok  '}  ${file}: ${kib(gzipped)} gzipped (${kib(content.length)} raw), ${share}% of ${budget} KiB`);
}

if (failed) {
    console.error('\nA file is over its size budget. Make it smaller, or raise the budget in tools/check-size.mjs if the growth is on purpose.');
    process.exit(1);
}
