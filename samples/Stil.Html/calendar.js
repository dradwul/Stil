// Renders a month view, a week planner and an agenda (Stil CSS: stil-calendar, stil-week,
// stil-agenda) from one list of events. stil.js adds the keyboard navigation in the month grids.
// This script is sample-only; StilCalendar, StilWeekPlanner and StilAgenda will do this in Blazor.
//
// The data model is the one planned for the components: an event has a start, an end, a
// title and a calendar (which gives it a variant). Everything else is derived: the load of a
// day, the free blocks in the working day and the layout of overlapping events.
(() => {
    const calendars = [
        { id: 'work', name: 'Work', variant: 'primary' },
        { id: 'customers', name: 'Customer meetings', variant: 'info' },
        { id: 'team', name: 'Team', variant: 'success' },
        { id: 'deadlines', name: 'Deadlines', variant: 'danger' },
        // A calendar can also have its own color instead of a variant.
        { id: 'personal', name: 'Personal', color: 'light-dark(#9c36b5, #da77f2)' },
    ];
    const calendarById = Object.fromEntries(calendars.map(c => [c.id, c]));

    // The working day, for the load and the free blocks, and the hours the week view shows.
    const workStart = 8 * 60;
    const workEnd = 17 * 60;
    const gridFrom = 7;
    const gridHours = 12;
    const minFree = 60;

    const locale = 'en-GB';
    const fmt = options => new Intl.DateTimeFormat(locale, options);
    const weekdayLong = fmt({ weekday: 'long' });
    const weekdayShort = fmt({ weekday: 'short' });
    const monthYear = fmt({ month: 'long', year: 'numeric' });
    const dayMonth = fmt({ day: 'numeric', month: 'short' });
    const dayMonthLong = fmt({ day: 'numeric', month: 'long' });

    // ---------- Dates ----------

    const startOfDay = date => new Date(date.getFullYear(), date.getMonth(), date.getDate());
    const addDays = (date, days) => new Date(date.getFullYear(), date.getMonth(), date.getDate() + days);
    const sameDay = (a, b) => a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();
    const mondayOf = date => addDays(startOfDay(date), -((date.getDay() + 6) % 7));
    const key = date => `${date.getFullYear()}-${date.getMonth() + 1}-${date.getDate()}`;
    const isWeekend = date => date.getDay() === 0 || date.getDay() === 6;
    const minutesOf = date => date.getHours() * 60 + date.getMinutes();
    const pad = n => String(n).padStart(2, '0');
    const time = minutes => `${pad(Math.floor(minutes / 60))}:${pad(minutes % 60)}`;
    const span = (start, end) => `${time(start)}–${time(end)}`;
    const hours = minutes => (minutes % 60 === 0 ? `${minutes / 60} h` : `${(minutes / 60).toFixed(1)} h`);

    // ISO 8601 week number: the week with the year's first Thursday is week 1.
    function isoWeek(date) {
        const thursday = addDays(mondayOf(date), 3);
        const firstThursday = addDays(mondayOf(new Date(thursday.getFullYear(), 0, 4)), 3);
        return 1 + Math.round((thursday - firstThursday) / (7 * 24 * 3600 * 1000));
    }

    const now = new Date();
    const today = startOfDay(now);
    const thisMonday = mondayOf(today);

    // ---------- Sample events ----------

    // The current week is written by hand so the week view always has something to show,
    // including overlaps and an evening event. Other days are generated from the date.
    const handWritten = [
        [0, '08:30', 30, 'Standup', 'team'],
        [0, '10:00', 90, 'Q4 planning', 'work', 'Room Fjord'],
        [0, '13:00', 60, 'Lunch with Kallio Design', 'customers', 'Helsinki'],
        [0, '15:00', 120, 'Supplier review', 'work'],
        [1, '08:30', 30, 'Standup', 'team'],
        [1, '09:00', 60, 'Design review', 'work', 'Room Skär'],
        [1, '11:00', 60, 'Call with Østerby Marine', 'customers', 'Video'],
        [1, '14:00', 30, '1:1 with Mikko', 'team'],
        [2, '08:30', 30, 'Standup', 'team'],
        [2, '09:30', 180, 'Workshop: new warehouse flow', 'work', 'Warehouse B'],
        [2, '10:00', 60, 'Visit from Hallberg & Söner', 'customers', 'Reception'],
        [2, '13:00', 60, 'Interview, junior buyer', 'team'],
        [2, '16:00', 30, 'Price list due', 'deadlines'],
        [3, '08:30', 30, 'Standup', 'team'],
        [3, '09:00', 120, 'Budget 2027', 'work', 'Room Fjord'],
        [3, '10:30', 60, 'Call with Fjällbacka Café', 'customers', 'Phone'],
        [3, '13:30', 90, 'Product photos', 'work', 'Studio'],
        [3, '17:30', 60, 'Running club', 'personal'],
        [4, '08:30', 30, 'Standup', 'team'],
        [4, '10:00', 60, 'Retrospective', 'team'],
        [4, '14:00', 30, 'Invoice run', 'deadlines'],
        [5, '10:00', 120, 'Christmas market planning', 'personal', 'Visby'],
    ];

    const templates = [
        [60, 'Customer call', 'customers', 'Video'],
        [90, 'Planning', 'work'],
        [60, 'Design review', 'work', 'Room Skär'],
        [30, '1:1', 'team'],
        [120, 'Supplier visit', 'customers', 'Warehouse B'],
        [60, 'Training', 'team'],
        [45, 'Order review', 'work'],
        [30, 'Report due', 'deadlines'],
    ];

    let nextId = 1;
    const cache = new Map();

    function makeEvent(date, start, length, title, calendar, location) {
        const [h, m] = start.split(':').map(Number);
        const from = new Date(date.getFullYear(), date.getMonth(), date.getDate(), h, m);
        return { id: nextId++, start: from, end: new Date(from.getTime() + length * 60000), title, calendar, location };
    }

    // A small deterministic generator, so a day always gets the same events.
    function random(seed) {
        let s = seed;
        return () => (s = (s * 1103515245 + 12345) % 2147483648) / 2147483648;
    }

    function eventsOn(date) {
        const k = key(date);
        if (cache.has(k)) return cache.get(k);

        let events;
        const offset = Math.round((startOfDay(date) - thisMonday) / 86400000);
        if (offset >= 0 && offset < 7) {
            events = handWritten.filter(e => e[0] === offset).map(e => makeEvent(date, ...e.slice(1)));
        } else if (isWeekend(date)) {
            const r = random(date.getFullYear() * 400 + date.getMonth() * 32 + date.getDate());
            events = r() > 0.75 ? [makeEvent(date, '11:00', 120, 'Family day', 'personal')] : [];
        } else {
            const r = random(date.getFullYear() * 400 + date.getMonth() * 32 + date.getDate());
            events = [makeEvent(date, '08:30', 30, 'Standup', 'team')];
            const count = Math.floor(r() * 4);
            const slots = ['09:00', '10:30', '13:00', '14:30', '15:30'];
            for (let i = 0; i < count; i++) {
                const t = templates[Math.floor(r() * templates.length)];
                const slot = slots.splice(Math.floor(r() * slots.length), 1)[0];
                events.push(makeEvent(date, slot, ...t));
            }
        }

        events.sort((a, b) => a.start - b.start);
        cache.set(k, events);
        return events;
    }

    // ---------- State ----------

    // On a phone the agenda is the default view; a view the user chose is remembered.
    const readView = () => { try { return localStorage.getItem('planner-view'); } catch { return null; } };
    const saveView = view => { try { localStorage.setItem('planner-view', view); } catch { } };
    const phone = window.matchMedia('(max-width: 575.98px)').matches;

    const state = {
        view: readView() ?? (phone ? 'agenda' : 'week'),
        month: new Date(today.getFullYear(), today.getMonth(), 1),
        week: thisMonday,
        selected: today,
        hidden: new Set(),
        drafts: [],
    };

    const visible = date => [...eventsOn(date), ...state.drafts.filter(d => sameDay(d.start, date))]
        .filter(e => !state.hidden.has(e.calendar))
        .sort((a, b) => a.start - b.start);

    // Booked minutes in the working day, merged so overlaps count once.
    function booked(date) {
        const ranges = visible(date)
            .map(e => [Math.max(workStart, minutesOf(e.start)), Math.min(workEnd, minutesOf(e.end))])
            .filter(([a, b]) => b > a)
            .sort((a, b) => a[0] - b[0]);
        let total = 0;
        let end = -1;
        for (const [a, b] of ranges) {
            if (a >= end) { total += b - a; end = b; } else if (b > end) { total += b - end; end = b; }
        }
        return total;
    }

    const load = date => Math.min(1, booked(date) / (workEnd - workStart));

    // Gaps of at least an hour in the working day, from now on.
    function freeBlocks(date) {
        if (isWeekend(date) || date < today) return [];
        let from = workStart;
        if (sameDay(date, today)) from = Math.max(from, Math.ceil(minutesOf(now) / 15) * 15);
        const blocks = [];
        for (const e of visible(date)) {
            const a = minutesOf(e.start);
            const b = minutesOf(e.end);
            if (a - from >= minFree && a <= workEnd) blocks.push({ date, start: from, end: Math.min(a, workEnd) });
            from = Math.max(from, b);
        }
        if (workEnd - from >= minFree) blocks.push({ date, start: from, end: workEnd });
        return blocks;
    }

    // Side-by-side columns for overlapping events: each cluster of overlaps shares its width.
    function layout(events) {
        const placed = [];
        let cluster = [];
        let clusterEnd = -1;
        const flush = () => {
            const cols = Math.max(1, ...cluster.map(p => p.col + 1));
            cluster.forEach(p => { p.cols = cols; });
            cluster = [];
        };
        for (const e of events) {
            const start = minutesOf(e.start);
            const end = minutesOf(e.end);
            if (start >= clusterEnd) { flush(); clusterEnd = end; } else { clusterEnd = Math.max(clusterEnd, end); }
            const taken = cluster.filter(p => p.end > start).map(p => p.col);
            let col = 0;
            while (taken.includes(col)) col++;
            const p = { e, start, end, col, cols: 1 };
            cluster.push(p);
            placed.push(p);
        }
        flush();
        return placed;
    }

    // ---------- Markup helpers ----------

    const html = (strings, ...values) => strings.reduce((out, s, i) => out + s + (i < values.length ? values[i] : ''), '');
    const esc = text => String(text).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
    // The class and style that color an event, a dot or a filter: a variant class, or its own color.
    const colorOf = calendar => calendar?.color
        ? { cls: '', style: `--stil-event-color: ${calendar.color}` }
        : { cls: ` stil-event-${calendar?.variant ?? 'primary'}`, style: '' };
    const styleAttr = (...styles) => (styles.some(Boolean) ? ` style="${styles.filter(Boolean).join('; ')}"` : '');
    const describe = e => `${e.title}, ${span(minutesOf(e.start), minutesOf(e.end))}, ${calendarById[e.calendar]?.name ?? ''}`;

    function eventButton(e, { meta = true, tabbable = true, startTime = false, metaTime = true, style = '' } = {}) {
        const draft = e.draft ? ' stil-event-draft' : '';
        const extra = [calendarById[e.calendar]?.name, e.location].filter(Boolean).join(' · ');
        const color = colorOf(calendarById[e.calendar]);
        return html`<button type="button" class="stil-event${color.cls}${draft}"${styleAttr(color.style, style)} data-event="${e.id}"
            aria-label="${esc(describe(e))}"${tabbable ? '' : ' tabindex="-1"'}>
            <span class="stil-event-title">${startTime ? `<span class="stil-event-time">${time(minutesOf(e.start))}</span>` : ''}${esc(e.title)}</span>
            ${meta ? html`<span class="stil-event-meta">${[metaTime ? span(minutesOf(e.start), minutesOf(e.end)) : '', esc(extra)].filter(Boolean).join(' · ')}</span>` : ''}
        </button>`;
    }

    const navButtons = (prev, next, { today = true } = {}) => html`<div class="stil-cluster stil-gap-1">
        <button type="button" class="stil-btn stil-btn-ghost stil-btn-sm stil-btn-icon" data-nav="-1" aria-label="${prev}">
            <svg aria-hidden="true" focusable="false" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m15 18-6-6 6-6"/></svg>
        </button>
        ${today ? '<button type="button" class="stil-btn stil-btn-outline stil-btn-secondary stil-btn-sm" data-nav="0">Today</button>' : ''}
        <button type="button" class="stil-btn stil-btn-ghost stil-btn-sm stil-btn-icon" data-nav="1" aria-label="${next}">
            <svg aria-hidden="true" focusable="false" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m9 18 6-6-6-6"/></svg>
        </button>
    </div>`;

    // ---------- Month view (and the mini month) ----------

    function renderMonth(container, month, { mini = false } = {}) {
        const first = mondayOf(month);
        const last = addDays(new Date(month.getFullYear(), month.getMonth() + 1, 0), 0);
        const weeks = Math.ceil((Math.round((last - first) / 86400000) + 1) / 7);
        const titleId = mini ? 'mini-month-title' : 'month-title';
        const weekDays = Array.from({ length: 7 }, (_, i) => addDays(first, i));

        let rows = '';
        for (let w = 0; w < weeks; w++) {
            const monday = addDays(first, w * 7);
            let cells = `<td class="stil-calendar-week" role="rowheader"><span class="stil-visually-hidden">Week </span>${isoWeek(monday)}</td>`;
            for (let d = 0; d < 7; d++) {
                const date = addDays(monday, d);
                const events = visible(date);
                const l = load(date);
                const outside = date.getMonth() !== month.getMonth();
                const isToday = sameDay(date, today);
                const selected = sameDay(date, state.selected);
                const inWeek = mini && date >= state.week && date < addDays(state.week, 7);
                const label = `${weekdayLong.format(date)} ${dayMonthLong.format(date)}, ` +
                    (events.length ? `${events.length} event${events.length > 1 ? 's' : ''}, ${Math.round(l * 100)}% booked` : 'free');
                const chips = events.slice(0, events.length > 3 ? 2 : 3)
                    .map(e => `<li>${eventButton(e, { meta: false, tabbable: false, startTime: true })}</li>`).join('');
                const more = events.length > 3
                    ? `<button type="button" class="stil-calendar-more" tabindex="-1" data-more="${key(date)}">+${events.length - 2} more</button>`
                    : '';
                const dots = events.slice(0, 5).map(e => { const c = colorOf(calendarById[e.calendar]); return `<span class="stil-calendar-dot${c.cls}"${styleAttr(c.style)}></span>`; }).join('');
                cells += html`<td role="gridcell" class="stil-calendar-day${outside ? ' stil-calendar-outside' : ''}${inWeek ? ' stil-calendar-range' : ''}"
                    style="--stil-load: ${l.toFixed(2)}" data-date="${key(date)}" tabindex="${selected ? 0 : -1}"
                    aria-selected="${selected}" aria-label="${label}"${isToday ? ' aria-current="date"' : ''}>
                    <span class="stil-calendar-date" aria-hidden="true">${date.getDate()}</span>
                    ${mini ? '' : `<ul class="stil-calendar-events">${chips}</ul>${more}<div class="stil-calendar-dots" aria-hidden="true">${dots}</div>`}
                </td>`;
            }
            rows += `<tr>${cells}</tr>`;
        }

        const heads = weekDays.map(d => `<th scope="col"><abbr title="${weekdayLong.format(d)}">${mini ? weekdayShort.format(d).slice(0, 2) : weekdayShort.format(d)}</abbr></th>`).join('');
        container.innerHTML = html`
            <div class="stil-calendar-header">
                <h2 class="stil-calendar-title" id="${titleId}">${monthYear.format(month)}</h2>
                ${navButtons('Previous month', 'Next month', { today: !mini })}
            </div>
            <table class="stil-calendar-grid" role="grid" aria-labelledby="${titleId}">
                <thead><tr><th scope="col" class="stil-calendar-week"><abbr title="Week number">${mini ? 'Wk' : 'Week'}</abbr></th>${heads}</tr></thead>
                <tbody>${rows}</tbody>
            </table>`;
    }

    // ---------- Week view ----------

    function renderWeek(container) {
        // The hours the grid shows are settings of stil-week.
        container.style.setProperty('--stil-week-first-hour', gridFrom);
        container.style.setProperty('--stil-week-hours', gridHours);
        const monday = state.week;
        const sunday = addDays(monday, 6);
        const days = Array.from({ length: 7 }, (_, i) => addDays(monday, i));

        const heads = days.map(date => {
            const b = booked(date);
            return html`<div class="stil-week-day-head"${sameDay(date, today) ? ' aria-current="date"' : ''}>
                <span class="stil-week-day-name">${weekdayShort.format(date)}</span>
                <span class="stil-calendar-date">${date.getDate()}</span>
                <span class="stil-week-load" title="${hours(b)} booked of ${hours(workEnd - workStart)}">
                    <span class="stil-week-load-bar" style="--stil-load: ${load(date).toFixed(2)}" aria-hidden="true"></span>
                    <span>${b ? hours(b) : 'Free'}</span>
                </span>
            </div>`;
        }).join('');

        const times = Array.from({ length: gridHours }, (_, h) =>
            `<span class="stil-week-time" style="--stil-start: ${(gridFrom + h) * 60}">${time((gridFrom + h) * 60)}</span>`).join('');

        const columns = days.map(date => {
            const gridStart = gridFrom * 60;
            const gridEnd = (gridFrom + gridHours) * 60;
            const events = layout(visible(date)).map(p => {
                const start = Math.max(gridStart, p.start);
                const end = Math.min(gridEnd, p.end);
                if (end <= start) return '';
                return eventButton(p.e, {
                    meta: end - start >= 45,
                    style: `--stil-start: ${start}; --stil-length: ${end - start}; --stil-col: ${p.col}; --stil-cols: ${p.cols}`,
                });
            }).join('');
            const free = freeBlocks(date).map(f => html`<button type="button" class="stil-week-free" style="--stil-start: ${f.start}; --stil-length: ${f.end - f.start}"
                data-free="${key(date)}|${f.start}|${f.end}" aria-label="Free ${span(f.start, f.end)}, create an event">Free · ${hours(f.end - f.start)}</button>`).join('');
            const nowLine = sameDay(date, today) && minutesOf(now) >= gridStart && minutesOf(now) <= gridEnd
                ? `<div class="stil-week-now" style="--stil-start: ${minutesOf(now)}" aria-hidden="true"></div>` : '';
            return `<div class="stil-week-col${isWeekend(date) ? ' stil-week-col-muted' : ''}">${free}${events}${nowLine}</div>`;
        }).join('');

        container.innerHTML = html`
            <div class="stil-calendar-header">
                <h2 class="stil-calendar-title" id="week-title">Week ${isoWeek(monday)}<small>${dayMonth.format(monday)} – ${dayMonth.format(sunday)} ${sunday.getFullYear()}</small></h2>
                ${navButtons('Previous week', 'Next week')}
            </div>
            <div class="stil-week-scroll" role="region" aria-labelledby="week-title" tabindex="0">
                <div class="stil-week-grid">
                    <div class="stil-week-corner" aria-hidden="true"></div>
                    ${heads}
                    <div class="stil-week-times" aria-hidden="true">${times}</div>
                    ${columns}
                </div>
            </div>`;
    }

    // ---------- Agenda ----------

    function renderAgenda(container) {
        let out = '';
        for (let i = 0; i < 7; i++) {
            const date = addDays(today, i);
            const events = visible(date);
            const free = freeBlocks(date);
            const name = i === 0 ? 'Today' : i === 1 ? 'Tomorrow' : weekdayLong.format(date);
            const items = [
                ...events.map(e => ({ at: minutesOf(e.start), html: html`<li class="stil-agenda-item">
                    <span class="stil-agenda-time">${span(minutesOf(e.start), minutesOf(e.end))}</span>${eventButton(e, { metaTime: false })}</li>` })),
                ...free.map(f => ({ at: f.start, html: html`<li class="stil-agenda-item stil-agenda-free">
                    <span class="stil-agenda-time">${span(f.start, f.end)}</span>
                    <span class="stil-agenda-free-line">Free · ${hours(f.end - f.start)}</span></li>` })),
            ].sort((a, b) => a.at - b.at).map(x => x.html).join('');
            const b = booked(date);
            out += html`<section class="stil-agenda-day" aria-labelledby="agenda-${i}">
                <h2 class="stil-agenda-day-title" id="agenda-${i}">${name} <small>${dayMonthLong.format(date)}${b ? ` · ${hours(b)} booked` : ''}</small></h2>
                ${items ? `<ul class="stil-agenda-list">${items}</ul>` : '<p class="stil-text-muted stil-fs-sm stil-mb-0">Nothing planned.</p>'}
            </section>`;
        }
        container.innerHTML = out;
    }

    // ---------- Side panels ----------

    function renderFreeList() {
        const blocks = Array.from({ length: 7 }, (_, i) => addDays(state.week, i))
            .flatMap(freeBlocks)
            .sort((a, b) => (b.end - b.start) - (a.end - a.start))
            .slice(0, 5)
            .sort((a, b) => a.date - b.date || a.start - b.start);
        document.querySelector('[data-free-list]').innerHTML = blocks.length
            ? blocks.map(f => html`<li><button type="button" data-free="${key(f.date)}|${f.start}|${f.end}" aria-label="Free ${weekdayLong.format(f.date)} ${span(f.start, f.end)}, create an event">
                <span>${weekdayShort.format(f.date)} ${span(f.start, f.end)}</span>
                <span class="planner-free-length">${hours(f.end - f.start)}</span></button></li>`).join('')
            : '<li class="stil-text-muted">No free hour left this week.</li>';
    }

    function renderFilters() {
        document.querySelector('[data-calendar-filters]').innerHTML = calendars.map(c => html`
            <label class="stil-calendar-filter${colorOf(c).cls}"${styleAttr(colorOf(c).style)}>
                <input type="checkbox" data-calendar="${c.id}"${state.hidden.has(c.id) ? '' : ' checked'}> ${c.name}
            </label>`).join('');
    }

    // ---------- Rendering and events ----------

    const monthEl = document.querySelector('[data-month]');
    const miniEl = document.querySelector('[data-mini-month]');
    const weekEl = document.querySelector('[data-week]');
    const agendaEl = document.querySelector('[data-agenda]');
    let miniMonth = new Date(today.getFullYear(), today.getMonth(), 1);

    function render() {
        renderMonth(monthEl, state.month);
        renderMonth(miniEl, miniMonth, { mini: true });
        renderWeek(weekEl);
        renderAgenda(agendaEl);
        renderFreeList();
        document.querySelectorAll('[role="tab"]').forEach(tab => {
            const selected = tab.dataset.view === state.view;
            tab.setAttribute('aria-selected', selected);
            tab.tabIndex = selected ? 0 : -1;
            document.getElementById(tab.getAttribute('aria-controls')).hidden = !selected;
        });
    }

    function toast(title, text, variant = 'info') {
        const host = document.querySelector('[data-toasts]');
        const el = document.createElement('div');
        el.className = `stil-toast stil-toast-${variant}`;
        el.setAttribute('role', 'status');
        el.innerHTML = html`<div class="stil-toast-body"><p class="stil-toast-title">${esc(title)}</p>${esc(text)}</div>
            <button type="button" class="stil-close" aria-label="Close"></button>`;
        el.querySelector('.stil-close').addEventListener('click', () => el.remove());
        host.append(el);
        setTimeout(() => el.remove(), 4000);
    }

    const parseKey = k => { const [y, m, d] = k.split('-').map(Number); return new Date(y, m - 1, d); };
    const findEvent = id => [...cache.values()].flat().concat(state.drafts).find(e => e.id === id);

    function showWeekOf(date) {
        state.week = mondayOf(date);
        state.selected = startOfDay(date);
        state.view = 'week';
        render();
    }

    document.addEventListener('click', event => {
        const target = event.target.closest('button, td[data-date]');
        if (!target) return;

        if (target.dataset.view) { state.view = target.dataset.view; saveView(state.view); render(); return; }

        if (target.dataset.nav !== undefined) {
            const step = Number(target.dataset.nav);
            if (target.closest('[data-mini-month]')) {
                miniMonth = step === 0 ? new Date(today.getFullYear(), today.getMonth(), 1) : new Date(miniMonth.getFullYear(), miniMonth.getMonth() + step, 1);
            } else if (target.closest('[data-month]')) {
                state.month = step === 0 ? new Date(today.getFullYear(), today.getMonth(), 1) : new Date(state.month.getFullYear(), state.month.getMonth() + step, 1);
            } else {
                state.week = step === 0 ? thisMonday : addDays(state.week, step * 7);
            }
            render();
            return;
        }

        if (target.dataset.event) {
            const e = findEvent(Number(target.dataset.event));
            if (e) toast(e.title, `${weekdayLong.format(e.start)} ${span(minutesOf(e.start), minutesOf(e.end))}${e.location ? ', ' + e.location : ''}`, 'info');
            return;
        }

        if (target.dataset.free) {
            // Clicking free time creates a draft event there (an hour, or less if the gap is shorter).
            const [k, start, end] = target.dataset.free.split('|');
            const date = parseKey(k);
            const length = Math.min(60, Number(end) - Number(start));
            const draft = makeEvent(date, time(Number(start)), length, 'New event', 'work');
            draft.draft = true;
            state.drafts.push(draft);
            state.week = mondayOf(date);
            state.view = 'week';
            render();
            toast('Draft created', `${weekdayLong.format(date)} ${span(Number(start), Number(start) + length)}. In the component, this opens the editor.`, 'success');
            return;
        }

        if (target.dataset.more) { showWeekOf(parseKey(target.dataset.more)); return; }

        if (target.dataset.date) {
            const date = parseKey(target.dataset.date);
            if (target.closest('[data-mini-month]') || sameDay(date, state.selected)) { showWeekOf(date); return; }
            state.selected = date;
            render();
            monthEl.querySelector(`td[data-date="${key(date)}"]`)?.focus();
        }
    });

    document.addEventListener('dblclick', event => {
        const cell = event.target.closest('[data-month] td[data-date]');
        if (cell) showWeekOf(parseKey(cell.dataset.date));
    });

    document.addEventListener('change', event => {
        const id = event.target.dataset.calendar;
        if (!id) return;
        if (event.target.checked) state.hidden.delete(id); else state.hidden.add(id);
        render();
    });

    // The arrow keys switch between the view tabs.
    document.addEventListener('keydown', event => {
        const tab = event.target.closest('[role="tab"]');
        if (!tab || (event.key !== 'ArrowRight' && event.key !== 'ArrowLeft')) return;
        const tabs = [...document.querySelectorAll('[role="tab"]')];
        const next = tabs[(tabs.indexOf(tab) + (event.key === 'ArrowRight' ? 1 : tabs.length - 1)) % tabs.length];
        state.view = next.dataset.view;
        saveView(state.view);
        render();
        next.focus();
        event.preventDefault();
    });

    // stil.js moves between the days with the arrow keys and clicks a day on Enter. Past the
    // first or last day it fires stilcalendaredge: show the previous or next month there.
    document.addEventListener('stilcalendaredge', event => {
        const { key: pressed, cell } = event.detail;
        const moves = { ArrowLeft: -1, ArrowRight: 1, ArrowUp: -7, ArrowDown: 7 };
        if (!(pressed in moves)) return;
        const next = addDays(parseKey(cell.dataset.date), moves[pressed]);
        const mini = !!cell.closest('[data-mini-month]');
        state.selected = next;
        if (mini) miniMonth = new Date(next.getFullYear(), next.getMonth(), 1);
        else state.month = new Date(next.getFullYear(), next.getMonth(), 1);
        render();
        (mini ? miniEl : monthEl).querySelector(`td[data-date="${key(next)}"]`)?.focus();
    });

    renderFilters();
    render();
})();
