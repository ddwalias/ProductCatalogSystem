import { q as head, n as escape_html, c as attr_class, r as attr, k as sanitize_props, l as spread_props, h as slot, d as stringify } from './index2-CRKCN0LV.js';
import { I as Icon } from './Icon-DIKcpDH_.js';
import { S as Search } from './search-DV1unkTP.js';

function Activity($$renderer, $$props) {
  const $$sanitized_props = sanitize_props($$props);
  const iconNode = [
    [
      "path",
      {
        "d": "M22 12h-2.48a2 2 0 0 0-1.93 1.46l-2.35 8.36a.25.25 0 0 1-.48 0L9.24 2.18a.25.25 0 0 0-.48 0l-2.35 8.36A2 2 0 0 1 4.49 12H2"
      }
    ]
  ];
  Icon($$renderer, spread_props([
    { name: "activity" },
    $$sanitized_props,
    {
      /**
       * @component @name Activity
       * @description Lucide SVG icon component, renders SVG Element with children.
       *
       * @preview ![img](data:image/svg+xml;base64,PHN2ZyAgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIgogIHdpZHRoPSIyNCIKICBoZWlnaHQ9IjI0IgogIHZpZXdCb3g9IjAgMCAyNCAyNCIKICBmaWxsPSJub25lIgogIHN0cm9rZT0iIzAwMCIgc3R5bGU9ImJhY2tncm91bmQtY29sb3I6ICNmZmY7IGJvcmRlci1yYWRpdXM6IDJweCIKICBzdHJva2Utd2lkdGg9IjIiCiAgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIgogIHN0cm9rZS1saW5lam9pbj0icm91bmQiCj4KICA8cGF0aCBkPSJNMjIgMTJoLTIuNDhhMiAyIDAgMCAwLTEuOTMgMS40NmwtMi4zNSA4LjM2YS4yNS4yNSAwIDAgMS0uNDggMEw5LjI0IDIuMThhLjI1LjI1IDAgMCAwLS40OCAwbC0yLjM1IDguMzZBMiAyIDAgMCAxIDQuNDkgMTJIMiIgLz4KPC9zdmc+Cg==) - https://lucide.dev/icons/activity
       * @see https://lucide.dev/guide/packages/lucide-svelte - Documentation
       *
       * @param {Object} props - Lucide icons props and any valid SVG attribute
       * @returns {FunctionalComponent} Svelte component
       *
       */
      iconNode,
      children: ($$renderer2) => {
        $$renderer2.push(`<!--[-->`);
        slot($$renderer2, $$props, "default", {});
        $$renderer2.push(`<!--]-->`);
      },
      $$slots: { default: true }
    }
  ]));
}
function Boxes($$renderer, $$props) {
  const $$sanitized_props = sanitize_props($$props);
  const iconNode = [
    [
      "path",
      {
        "d": "M2.97 12.92A2 2 0 0 0 2 14.63v3.24a2 2 0 0 0 .97 1.71l3 1.8a2 2 0 0 0 2.06 0L12 19v-5.5l-5-3-4.03 2.42Z"
      }
    ],
    ["path", { "d": "m7 16.5-4.74-2.85" }],
    ["path", { "d": "m7 16.5 5-3" }],
    ["path", { "d": "M7 16.5v5.17" }],
    [
      "path",
      {
        "d": "M12 13.5V19l3.97 2.38a2 2 0 0 0 2.06 0l3-1.8a2 2 0 0 0 .97-1.71v-3.24a2 2 0 0 0-.97-1.71L17 10.5l-5 3Z"
      }
    ],
    ["path", { "d": "m17 16.5-5-3" }],
    ["path", { "d": "m17 16.5 4.74-2.85" }],
    ["path", { "d": "M17 16.5v5.17" }],
    [
      "path",
      {
        "d": "M7.97 4.42A2 2 0 0 0 7 6.13v4.37l5 3 5-3V6.13a2 2 0 0 0-.97-1.71l-3-1.8a2 2 0 0 0-2.06 0l-3 1.8Z"
      }
    ],
    ["path", { "d": "M12 8 7.26 5.15" }],
    ["path", { "d": "m12 8 4.74-2.85" }],
    ["path", { "d": "M12 13.5V8" }]
  ];
  Icon($$renderer, spread_props([
    { name: "boxes" },
    $$sanitized_props,
    {
      /**
       * @component @name Boxes
       * @description Lucide SVG icon component, renders SVG Element with children.
       *
       * @preview ![img](data:image/svg+xml;base64,PHN2ZyAgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIgogIHdpZHRoPSIyNCIKICBoZWlnaHQ9IjI0IgogIHZpZXdCb3g9IjAgMCAyNCAyNCIKICBmaWxsPSJub25lIgogIHN0cm9rZT0iIzAwMCIgc3R5bGU9ImJhY2tncm91bmQtY29sb3I6ICNmZmY7IGJvcmRlci1yYWRpdXM6IDJweCIKICBzdHJva2Utd2lkdGg9IjIiCiAgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIgogIHN0cm9rZS1saW5lam9pbj0icm91bmQiCj4KICA8cGF0aCBkPSJNMi45NyAxMi45MkEyIDIgMCAwIDAgMiAxNC42M3YzLjI0YTIgMiAwIDAgMCAuOTcgMS43MWwzIDEuOGEyIDIgMCAwIDAgMi4wNiAwTDEyIDE5di01LjVsLTUtMy00LjAzIDIuNDJaIiAvPgogIDxwYXRoIGQ9Im03IDE2LjUtNC43NC0yLjg1IiAvPgogIDxwYXRoIGQ9Im03IDE2LjUgNS0zIiAvPgogIDxwYXRoIGQ9Ik03IDE2LjV2NS4xNyIgLz4KICA8cGF0aCBkPSJNMTIgMTMuNVYxOWwzLjk3IDIuMzhhMiAyIDAgMCAwIDIuMDYgMGwzLTEuOGEyIDIgMCAwIDAgLjk3LTEuNzF2LTMuMjRhMiAyIDAgMCAwLS45Ny0xLjcxTDE3IDEwLjVsLTUgM1oiIC8+CiAgPHBhdGggZD0ibTE3IDE2LjUtNS0zIiAvPgogIDxwYXRoIGQ9Im0xNyAxNi41IDQuNzQtMi44NSIgLz4KICA8cGF0aCBkPSJNMTcgMTYuNXY1LjE3IiAvPgogIDxwYXRoIGQ9Ik03Ljk3IDQuNDJBMiAyIDAgMCAwIDcgNi4xM3Y0LjM3bDUgMyA1LTNWNi4xM2EyIDIgMCAwIDAtLjk3LTEuNzFsLTMtMS44YTIgMiAwIDAgMC0yLjA2IDBsLTMgMS44WiIgLz4KICA8cGF0aCBkPSJNMTIgOCA3LjI2IDUuMTUiIC8+CiAgPHBhdGggZD0ibTEyIDggNC43NC0yLjg1IiAvPgogIDxwYXRoIGQ9Ik0xMiAxMy41VjgiIC8+Cjwvc3ZnPgo=) - https://lucide.dev/icons/boxes
       * @see https://lucide.dev/guide/packages/lucide-svelte - Documentation
       *
       * @param {Object} props - Lucide icons props and any valid SVG attribute
       * @returns {FunctionalComponent} Svelte component
       *
       */
      iconNode,
      children: ($$renderer2) => {
        $$renderer2.push(`<!--[-->`);
        slot($$renderer2, $$props, "default", {});
        $$renderer2.push(`<!--]-->`);
      },
      $$slots: { default: true }
    }
  ]));
}
function Database($$renderer, $$props) {
  const $$sanitized_props = sanitize_props($$props);
  const iconNode = [
    ["ellipse", { "cx": "12", "cy": "5", "rx": "9", "ry": "3" }],
    ["path", { "d": "M3 5V19A9 3 0 0 0 21 19V5" }],
    ["path", { "d": "M3 12A9 3 0 0 0 21 12" }]
  ];
  Icon($$renderer, spread_props([
    { name: "database" },
    $$sanitized_props,
    {
      /**
       * @component @name Database
       * @description Lucide SVG icon component, renders SVG Element with children.
       *
       * @preview ![img](data:image/svg+xml;base64,PHN2ZyAgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIgogIHdpZHRoPSIyNCIKICBoZWlnaHQ9IjI0IgogIHZpZXdCb3g9IjAgMCAyNCAyNCIKICBmaWxsPSJub25lIgogIHN0cm9rZT0iIzAwMCIgc3R5bGU9ImJhY2tncm91bmQtY29sb3I6ICNmZmY7IGJvcmRlci1yYWRpdXM6IDJweCIKICBzdHJva2Utd2lkdGg9IjIiCiAgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIgogIHN0cm9rZS1saW5lam9pbj0icm91bmQiCj4KICA8ZWxsaXBzZSBjeD0iMTIiIGN5PSI1IiByeD0iOSIgcnk9IjMiIC8+CiAgPHBhdGggZD0iTTMgNVYxOUE5IDMgMCAwIDAgMjEgMTlWNSIgLz4KICA8cGF0aCBkPSJNMyAxMkE5IDMgMCAwIDAgMjEgMTIiIC8+Cjwvc3ZnPgo=) - https://lucide.dev/icons/database
       * @see https://lucide.dev/guide/packages/lucide-svelte - Documentation
       *
       * @param {Object} props - Lucide icons props and any valid SVG attribute
       * @returns {FunctionalComponent} Svelte component
       *
       */
      iconNode,
      children: ($$renderer2) => {
        $$renderer2.push(`<!--[-->`);
        slot($$renderer2, $$props, "default", {});
        $$renderer2.push(`<!--]-->`);
      },
      $$slots: { default: true }
    }
  ]));
}
function _page($$renderer, $$props) {
  $$renderer.component(($$renderer2) => {
    let healthMessage = "Checking...";
    let totalProducts = 0;
    let activeCategoryCount = 0;
    let searchInput = "";
    head("1uha8ag", $$renderer2, ($$renderer3) => {
      $$renderer3.title(($$renderer4) => {
        $$renderer4.push(`<title>Dashboard - Catalog Studio</title>`);
      });
    });
    $$renderer2.push(`<div class="space-y-8 animate-in fade-in duration-500"><header><h1 class="text-3xl font-bold tracking-tight text-white mb-2">Command Deck</h1> <p class="text-slate-400">System overview and global search</p></header> <div class="grid grid-cols-1 md:grid-cols-3 gap-6"><div class="glass-card p-6 flex items-start gap-4"><div class="p-3 bg-indigo-500/10 text-indigo-400 rounded-lg">`);
    Boxes($$renderer2, { size: 24 });
    $$renderer2.push(`<!----></div> <div><p class="text-sm text-slate-400 font-medium">Total Products</p> <p class="text-2xl font-bold text-white mt-1">${escape_html(totalProducts)}</p></div></div> <div class="glass-card p-6 flex items-start gap-4"><div class="p-3 bg-cyan-500/10 text-cyan-400 rounded-lg">`);
    Database($$renderer2, { size: 24 });
    $$renderer2.push(`<!----></div> <div><p class="text-sm text-slate-400 font-medium">Active Categories</p> <p class="text-2xl font-bold text-white mt-1">${escape_html(activeCategoryCount)}</p></div></div> <div class="glass-card p-6 flex items-start gap-4"><div${attr_class(`p-3 ${stringify("bg-rose-500/10 text-rose-400")} rounded-lg`)}>`);
    Activity($$renderer2, { size: 24 });
    $$renderer2.push(`<!----></div> <div><p class="text-sm text-slate-400 font-medium">API Health</p> <p class="text-lg font-bold text-white mt-1 truncate max-w-[150px]"${attr("title", healthMessage)}>${escape_html(healthMessage)}</p></div></div></div> <div class="glass-panel p-8"><h2 class="text-xl font-bold text-white mb-6">Global Search</h2> <div class="relative max-w-2xl"><div class="relative">`);
    Search($$renderer2, {
      class: "absolute left-4 top-1/2 -translate-y-1/2 text-slate-400",
      size: 20
    });
    $$renderer2.push(`<!----> <input${attr("value", searchInput)} type="text" placeholder="Search for products across the entire catalog..." class="w-full bg-slate-950/50 border border-slate-700/50 rounded-xl py-4 pl-12 pr-4 text-slate-100 placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/50 transition-all text-lg shadow-inner"/></div> `);
    {
      $$renderer2.push("<!--[-1-->");
    }
    $$renderer2.push(`<!--]--></div></div></div>`);
  });
}

export { _page as default };
//# sourceMappingURL=_page.svelte-C7MmM6P0.js.map
