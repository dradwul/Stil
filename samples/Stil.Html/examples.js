// Showcase-only: shows the markup of every .example as a code block above it, so the
// examples and their code can never drift apart.
(() => {
    const escape = text => text.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');

    function dedent(html) {
        const lines = html.replace(/^\s*\n/, '').replace(/\s+$/, '').split('\n');
        const indents = lines.filter(line => line.trim()).map(line => line.match(/^\s*/)[0].length);
        const common = Math.min(...indents);
        return lines.map(line => line.slice(common)).join('\n');
    }

    document.querySelectorAll('.example').forEach(example => {
        const pre = document.createElement('pre');
        pre.className = 'example-code';
        // Long lines scroll sideways, so keyboard users must be able to focus the block.
        pre.tabIndex = 0;
        const markup = dedent(example.innerHTML).replace(/=""/g, '');
        pre.innerHTML = `<code>${escape(markup)}</code>`;
        example.before(pre);
    });
})();
