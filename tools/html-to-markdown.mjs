// Turns a rendered docs page into Markdown, for llms.txt and the pages' .md copies. It needs no
// package: the HTML is Blazor's own output, so a small parser is enough. Only the page's <main>
// is kept, without the live previews of the examples (their source is kept), the copy buttons and
// the previous/next links.

const VOID = new Set(['area', 'base', 'br', 'col', 'embed', 'hr', 'img', 'input', 'link', 'meta', 'source', 'track', 'wbr']);
const RAW = new Set(['script', 'style', 'textarea']);
const SKIP = new Set(['script', 'style', 'svg', 'template', 'button', 'nav', 'aside', 'summary', 'input', 'select', 'textarea', 'img', 'canvas', 'video', 'iframe', 'noscript']);
const SKIP_CLASSES = [
    'docs-example-preview', 'docs-code-bar', 'docs-pager', 'docs-eyebrow', 'stil-visually-hidden',
    // The theme builder's controls and preview: the page's text and its code are what matter.
    'docs-theme-controls', 'docs-preview-frame',
];
const BLOCK = new Set([
    'address', 'article', 'blockquote', 'caption', 'dd', 'details', 'dialog', 'div', 'dl', 'dt', 'fieldset', 'figcaption',
    'figure', 'footer', 'form', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'header', 'hr', 'label', 'legend', 'li', 'main', 'ol', 'p', 'pre',
    'section', 'table', 'tbody', 'td', 'tfoot', 'th', 'thead', 'tr', 'ul',
]);
const ENTITIES = { amp: '&', lt: '<', gt: '>', quot: '"', apos: "'", nbsp: ' ', times: '×', hellip: '…', mdash: '—', ndash: '–', copy: '©' };

/** The Markdown of a page's <main>, with links made absolute against the page's address. */
export function pageToMarkdown(html, pageUrl) {
    const root = parse(html);
    const main = find(root, node => node.tag === 'main');
    if (!main) {
        throw new Error(`${pageUrl} has no <main>.`);
    }

    const ids = new Map();
    const index = node => {
        if (node.attrs?.id) {
            ids.set(node.attrs.id, node);
        }

        node.children?.forEach(index);
    };
    index(root);
    // Relative links are relative to <base href>, as in the browser; #anchors to the page itself.
    const base = find(root, node => node.tag === 'base' && node.attrs.href !== undefined);
    const baseUrl = base ? new URL(base.attrs.href, pageUrl).href : pageUrl;
    return blocksOf(main.children, { pageUrl, baseUrl, ids }).join('\n\n') + '\n';
}

/** The document as a tree of { tag, attrs, children } and { text } nodes. */
export function parse(html) {
    const root = { tag: '#root', attrs: {}, children: [] };
    const stack = [root];
    const token = /<!--[\s\S]*?-->|<!\w[^>]*>|<\/([a-zA-Z][\w:-]*)\s*>|<([a-zA-Z][\w:-]*)((?:\s+[^\s=/>]+(?:\s*=\s*(?:"[^"]*"|'[^']*'|[^\s>]+))?)*)\s*(\/?)>|[^<]+|</g;
    let match;
    while ((match = token.exec(html))) {
        const [whole, close, open, attrs, selfClosing] = match;
        const parent = stack[stack.length - 1];
        if (close) {
            const name = close.toLowerCase();
            const index = stack.findLastIndex(node => node.tag === name);
            if (index > 0) {
                stack.length = index;
            }
        } else if (open) {
            const node = { tag: open.toLowerCase(), attrs: parseAttributes(attrs), children: [] };
            parent.children.push(node);
            if (RAW.has(node.tag)) {
                const end = html.indexOf(`</${node.tag}`, token.lastIndex);
                const stop = end < 0 ? html.length : end;
                node.children.push({ text: decode(html.slice(token.lastIndex, stop)) });
                token.lastIndex = stop;
            } else if (!selfClosing && !VOID.has(node.tag)) {
                stack.push(node);
            }
        } else if (!whole.startsWith('<!')) {
            parent.children.push({ text: decode(whole) });
        }
    }

    return root;
}

function parseAttributes(source) {
    const attrs = {};
    for (const [, name, double, single, bare] of source.matchAll(/([^\s=/>]+)(?:\s*=\s*(?:"([^"]*)"|'([^']*)'|([^\s>]+)))?/g)) {
        attrs[name.toLowerCase()] = decode(double ?? single ?? bare ?? '');
    }

    return attrs;
}

function decode(text) {
    return text.replace(/&(#x[0-9a-f]+|#\d+|[a-z]+);/gi, (entity, code) => {
        if (code[0] === '#') {
            const value = code[1] === 'x' || code[1] === 'X' ? parseInt(code.slice(2), 16) : parseInt(code.slice(1), 10);
            return String.fromCodePoint(value);
        }

        return ENTITIES[code.toLowerCase()] ?? entity;
    });
}

function find(node, test) {
    if (test(node)) {
        return node;
    }

    for (const child of node.children ?? []) {
        const found = find(child, test);
        if (found) {
            return found;
        }
    }

    return null;
}

function hasClass(node, name) {
    return (node.attrs?.class ?? '').split(/\s+/).includes(name);
}

function skipped(node) {
    return SKIP.has(node.tag)
        || node.attrs['aria-hidden'] === 'true'
        || ('hidden' in node.attrs && node.attrs.role !== 'tabpanel')
        || SKIP_CLASSES.some(name => hasClass(node, name));
}

function isBlock(node) {
    return BLOCK.has(node.tag);
}

// Block content: a list of Markdown blocks, to be joined with blank lines.
function blocksOf(children, context) {
    const blocks = [];
    let inline = '';
    const flush = () => {
        const text = tidy(inline);
        if (text) {
            blocks.push(text);
        }

        inline = '';
    };

    for (const node of children) {
        if (node.text !== undefined) {
            inline += node.text;
        } else if (skipped(node)) {
            continue;
        } else if (isBlock(node)) {
            flush();
            blocks.push(...block(node, context));
        } else {
            inline += inlineOf(node, context);
        }
    }

    flush();
    return blocks;
}

function block(node, context) {
    const heading = /^h([1-6])$/.exec(node.tag);
    if (heading) {
        const text = tidy(inlineOf(node, context));
        return text ? [`${'#'.repeat(Number(heading[1]))} ${text}`] : [];
    }

    if (hasClass(node, 'docs-code')) {
        const language = textOf(find(node, child => hasClass(child, 'docs-code-language')) ?? { children: [] }).trim();
        const pre = find(node, child => child.tag === 'pre');
        return pre ? [fence(textOf(pre), language)] : [];
    }

    // Every tab's panel is kept (the hidden ones too), under the name of its tab.
    if (node.attrs.role === 'tabpanel') {
        const tab = context.ids.get(node.attrs['aria-labelledby']);
        const title = tab ? tidy(textOf(tab)) : '';
        const content = blocksOf(node.children, context);
        return content.length && title ? [`**${title}**`, ...content] : content;
    }

    switch (node.tag) {
        case 'p':
        case 'label':
        case 'dt':
        case 'dd':
        case 'caption':
        case 'figcaption':
        case 'legend': {
            const text = tidy(inlineOf(node, context));
            return text ? [text] : [];
        }
        case 'pre':
            return [fence(textOf(node), '')];
        case 'hr':
            return ['---'];
        case 'ul':
        case 'ol':
            return list(node, context);
        case 'table':
            return table(node, context);
        case 'blockquote':
            return [blocksOf(node.children, context).join('\n\n').replace(/^/gm, '> ')];
        default:
            return blocksOf(node.children, context);
    }
}

function list(node, context) {
    const items = node.children.filter(child => child.tag === 'li' && !skipped(child));
    const lines = items.map((item, index) => {
        const marker = node.tag === 'ol' ? `${index + 1}. ` : '- ';
        const content = blocksOf(item.children, context).join('\n');
        return marker + content.replace(/\n/g, `\n${' '.repeat(marker.length)}`);
    });
    return lines.length ? [lines.join('\n')] : [];
}

function table(node, context) {
    const rows = [];
    const collect = parent => {
        for (const child of parent.children ?? []) {
            if (child.tag === 'tr') {
                rows.push(child.children.filter(cell => cell.tag === 'th' || cell.tag === 'td')
                    .map(cell => tidy(inlineOf(cell, context)).replace(/\n/g, ' ').replaceAll('|', '\\|')));
            } else if (child.tag && !skipped(child) && child.tag !== 'table') {
                collect(child);
            }
        }
    };
    collect(node);
    if (rows.length === 0) {
        return [];
    }

    const width = Math.max(...rows.map(row => row.length));
    const line = row => `| ${Array.from({ length: width }, (_, i) => row[i] ?? '').join(' | ')} |`;
    return [[line(rows[0]), line(Array(width).fill('---')), ...rows.slice(1).map(line)].join('\n')];
}

// Inline content, with whitespace kept for tidy() to collapse; \n marks a <br>.
function inlineOf(node, context) {
    if (node.text !== undefined) {
        return node.text.replace(/\s+/g, ' ');
    }

    if (skipped(node)) {
        return '';
    }

    const inner = () => node.children.map(child => inlineOf(child, context)).join('');
    if (hasClass(node, 'stil-list-item-content')) {
        // A list row's title and description.
        return node.children.map(child => tidy(inlineOf(child, context))).filter(Boolean).join(': ');
    }

    if (hasClass(node, 'docs-choice-title')) {
        // A choice in the project wizard: its name, then what it gives.
        const text = tidy(inner());
        return text ? `**${text}**: ` : '';
    }

    switch (node.tag) {
        case 'br':
            return '\n';
        case 'code':
        case 'kbd':
            return code(textOf(node).replace(/\s+/g, ' ').trim());
        case 'strong':
        case 'b': {
            const text = inner().trim();
            return text ? `**${text}**` : '';
        }
        case 'em': {
            const text = inner().trim();
            return text ? `*${text}*` : '';
        }
        case 'a': {
            const text = inner().trim();
            const href = node.attrs.href;
            return text && href ? `[${text}](${resolve(href, context)})` : text;
        }
        default:
            return isBlock(node) ? ` ${inner()} ` : inner();
    }
}

function code(text) {
    if (!text) {
        return '';
    }

    const ticks = text.includes('`') ? '``' : '`';
    return ticks.length > 1 ? `${ticks} ${text} ${ticks}` : `${ticks}${text}${ticks}`;
}

function fence(text, language) {
    const body = text.replace(/\r\n/g, '\n').replace(/^\n+|\s+$/g, '');
    const ticks = body.includes('```') ? '````' : '```';
    return `${ticks}${language}\n${body}\n${ticks}`;
}

function textOf(node) {
    if (node.text !== undefined) {
        return node.text;
    }

    return (node.children ?? []).map(textOf).join('');
}

function resolve(href, { pageUrl, baseUrl }) {
    if (href.startsWith('#')) {
        return pageUrl.split('#')[0] + href;
    }

    try {
        return new URL(href, baseUrl).href;
    } catch {
        return href;
    }
}

function tidy(text) {
    return text.split('\n').map(line => line.replace(/\s+/g, ' ').trim()).filter(Boolean).join('\n');
}
